﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.BinaryStorageProvider;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using Dfc.CourseDirectory.Core.Search.AzureSearch;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.WebV2.AddressSearch;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.Cookies;
using Dfc.CourseDirectory.WebV2.FeatureFlagProviders;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.ModelBinding;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Dfc.CourseDirectory.WebV2.Security;
using Dfc.CourseDirectory.WebV2.Security.AuthorizationPolicies;
using Dfc.CourseDirectory.WebV2.TagHelpers;
using Dfc.CourseDirectory.WebV2.ViewHelpers;
using FormFlow;
using GovUk.Frontend.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Dfc.CourseDirectory.WebV2
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCourseDirectory(
            this IServiceCollection services,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            var thisAssembly = typeof(ServiceCollectionExtensions).Assembly;

            services
                .AddMvc(options =>
                {
                    options.EnableEndpointRouting = true;

                    options.Conventions.Add(new AddFeaturePropertyModelConvention());
                    options.Conventions.Add(new V2ActionModelConvention());

                    options.Filters.Add(new RedirectToProviderSelectionActionFilter());
                    options.Filters.Add(new VerifyApprenticeshipIdActionFilter());
                    options.Filters.Add(new DeactivatedProviderErrorActionFilter());
                    options.Filters.Add(new NotAuthorizedExceptionFilter());
                    options.Filters.Add(new InvalidStateExceptionFilter());
                    options.Filters.Add(new LocalUrlActionFilter());
                    options.Filters.Add(new MptxResourceFilter());
                    options.Filters.Add(new ContentSecurityPolicyActionFilter());
                    options.Filters.Add(new MptxControllerActionFilter());
                    options.Filters.Add(new InvalidMptxInstanceContextActionFilter());
                    options.Filters.Add(new ResourceDoesNotExistExceptionFilter());
                    options.Filters.Add(new StateExpiredExceptionFilter());

                    // If a binder type is is explicitly specified then ensure it's honoured
                    Debug.Assert(options.ModelBinderProviders[0].GetType() == typeof(BinderTypeModelBinderProvider));
                    options.ModelBinderProviders.Insert(1, new MptxInstanceContextModelBinderProvider());
                    options.ModelBinderProviders.Insert(1, new MultiValueEnumModelBinderProvider());
                    options.ModelBinderProviders.Insert(1, new StandardModelBinderProvider());
                })
                .AddApplicationPart(thisAssembly)
                .AddRazorOptions(options =>
                {
                    // TODO When the legacy views are all moved this check can go away
                    if (environment.IsTesting())
                    {
                        options.ViewLocationFormats.Clear();
                    }

                    options.ViewLocationFormats.Add("/SharedViews/{0}.cshtml");

                    options.ViewLocationExpanders.Add(new FeatureViewLocationExpander());
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    AuthorizationPolicyNames.Admin,
                    policy => policy.RequireRole(RoleNames.Developer, RoleNames.Helpdesk));
            });

            // SignInActions - order here is the order they're executed in
            services.AddTransient<ISignInAction, DfeUserInfoHelper>();
            services.AddTransient<ISignInAction, EnsureProviderExistsSignInAction>();
            services.AddTransient<ISignInAction, SignInTracker>();
            services.AddTransient<ISignInAction, EnsureApprenticeshipQAStatusSetSignInAction>();

            services.AddSqlDataStore(configuration.GetConnectionString("DefaultConnection"));

            if (!environment.IsTesting())
            {
                services.AddCosmosDbDataStore(
                    endpoint: new Uri(configuration["CosmosDbSettings:EndpointUri"]),
                    key: configuration["CosmosDbSettings:PrimaryKey"]);

                services.AddSingleton<IBinaryStorageProvider, BlobStorageBinaryStorageProvider>();
            }

            // HostedService to execute startup tasks.
            // N.B. it's important this is the first HostedService to run; it may set up dependencies for other services.
            services.Insert(
                0,
                new ServiceDescriptor(typeof(IHostedService), typeof(RunStartupTasksHostedService),
                ServiceLifetime.Transient));

            services.AddFormFlow();
            services.AddJourneyStateTypes(typeof(ServiceCollectionExtensions).Assembly);

            services.AddTransient<IProviderOwnershipCache, ProviderOwnershipCache>();
            services.AddSingleton<IProviderInfoCache, ProviderInfoCache>();
            services.AddGovUkFrontend(new GovUkFrontendAspNetCoreOptions()
            {
                // Avoid import being added to old pages
                AddImportsToHtml = false,
                DateInputModelConverters =
                {
                    new ModelBinding.DateInputModelConverter()
                }
            });
            services.AddMediatR(typeof(ServiceCollectionExtensions));
            services.AddTransient<IClock, SystemClock>();
            services.AddSingleton<ICurrentUserProvider, ClaimsPrincipalCurrentUserProvider>();
            services.AddHttpContextAccessor();
            services.TryAddScoped<IFeatureFlagProvider, ConfigurationFeatureFlagProvider>();
            services.Decorate<IFeatureFlagProvider, DataManagementFeatureFlagProvider>();
            services.AddScoped<SignInTracker>();
            services.AddBehaviors(typeof(ServiceCollectionExtensions).Assembly);
            services.AddSingleton<IStandardsCache, StandardsCache>();
            services.AddSingleton<MptxInstanceProvider>();
            services.AddMptxInstanceContext();
            services.AddSingleton<IMptxStateProvider, SessionMptxStateProvider>();
            services.AddSingleton<MptxInstanceContextFactory>();
            services.AddSingleton<IProviderContextProvider, ProviderContextProvider>();
            services.AddSingleton(new AddressSearch.Options() { Key = configuration["PostCodeSearchSettings:Key"] });
            services.Configure<GetAddressAddressSearchServiceOptions>(configuration.GetSection("GetAddressSettings"));
            services.AddTransient<UkrlpSyncHelper>();
            services.AddTransient<IUkrlpService, Core.ReferenceData.Ukrlp.UkrlpService>();
            services.AddTransient<MptxManager>();
            services.AddTransient<Features.NewApprenticeshipProvider.FlowModelInitializer>();
            services.AddTransient<ITagHelperComponent, AppendMptxInstanceTagHelperComponent>();
            services.AddTransient<Features.ApprenticeshipQA.ProviderAssessment.JourneyModelInitializer>();
            services.AddTransient<Features.ApprenticeshipQA.ApprenticeshipAssessment.JourneyModelInitializer>();
            services.Configure<Settings>(configuration);
            services.AddSingleton<Settings>(sp => sp.GetRequiredService<IOptions<Settings>>().Value);
            services.AddScoped<ICookieSettingsProvider, CookieSettingsProvider>();
            services.AddTransient<ITagHelperComponent, AnalyticsTagHelperComponent>();
            services.Configure<ApprenticeshipBulkUploadSettings>(configuration.GetSection("ApprenticeshipBulkUpload"));
            services.AddTransient<ProviderContextHelper>();
            services.AddTransient<Features.Venues.EditVenue.EditVenueJourneyModelFactory>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IAuthorizationHandler, ProviderTypeAuthorizationHandler>();
            services.Configure<GoogleAnalyticsOptions>(configuration.GetSection("GoogleAnalytics"));
            services.Configure<GoogleTagManagerOptions>(configuration.GetSection("GoogleTagManager"));
            services.AddTransient<SqlDataSync>();
            services.AddScoped<RouteValuesHelper>();
            services.AddTransient<Features.TLevels.ViewAndEditTLevel.EditTLevelJourneyModelFactory>();
            services.AddSingleton<IRegionCache, RegionCache>();
            services.AddTransient<IFileUploadProcessor, FileUploadProcessor>();

            if (!environment.IsTesting())
            {
				services.AddAzureSearchClient<Provider>(
	                new Uri(configuration["AzureSearchUrl"]),
	                configuration["AzureSearchQueryKey"],
	                configuration["ProviderAzureSearchIndexName"]);

                services.AddAzureSearchClient<Lars>(
                    new Uri(configuration["AzureSearchUrl"]),
                    configuration["AzureSearchQueryKey"],
                    configuration["LarsAzureSearchIndexName"]);

                services.AddSingleton<IAddressSearchService>(s =>
                {
                    var getAddressOptions = s.GetRequiredService<IOptions<GetAddressAddressSearchServiceOptions>>();

                    if (getAddressOptions.Value.UseGetAddress)
                    {
                        return new GetAddressAddressSearchService(s.GetRequiredService<HttpClient>(), getAddressOptions);
                    }

                    return new LoqateAddressSearchService(s.GetRequiredService<HttpClient>(), s.GetRequiredService<AddressSearch.Options>());
                });

                services.AddSingleton(new BlobServiceClient(configuration["BlobStorageSettings:ConnectionString"]));
            }

#if DEBUG
            if (configuration["UseLocalFileMptxStateProvider"]?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                services.AddSingleton<IMptxStateProvider, LocalFileMptxStateProvider>();
            }
#endif

            return services;
        }

        public static void AddDfeSignIn(this IServiceCollection services, DfeSignInSettings settings)
        {
            var overallSessionTimeout = TimeSpan.FromMinutes(90);

            services.AddSingleton<DfeUserInfoHelper>();
            // N.B. We can't use a typed client here because the legacy bits override the HttpClient registration :-/
            services.AddHttpClient("DfeSignIn", client =>
            {
                client.BaseAddress = new Uri(settings.ApiBaseUri);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {DfeUserInfoHelper.CreateApiToken(settings)}");
            });
            services.TryAddSingleton(settings);

            services
                .AddAuthentication(options =>
                {
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(40);
                    options.SlidingExpiration = true;
                    options.LogoutPath = "/auth/logout";

                    options.Events.OnRedirectToAccessDenied = ctx =>
                    {
                        ctx.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    };
                })
                .AddOpenIdConnect(options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.MetadataAddress = settings.MetadataAddress;
                    options.RequireHttpsMetadata = false;
                    options.ClientId = settings.ClientId;
                    options.ClientSecret = settings.ClientSecret;
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("email");
                    options.Scope.Add("profile");
                    options.Scope.Add("organisationid");
                    options.Scope.Add("offline_access");

                    // When we expire the session, ensure user is prompted to sign in again at DfE Sign In
                    options.MaxAge = overallSessionTimeout;

                    options.SaveTokens = true;
                    options.CallbackPath = settings.CallbackPath;
                    options.SignedOutCallbackPath = settings.SignedOutCallbackPath;
                    options.SecurityTokenValidator = new JwtSecurityTokenHandler()
                    {
                        InboundClaimTypeMap = new Dictionary<string, string>(),
                        TokenLifetimeInMinutes = 90,
                        SetDefaultTimesOnTokenCreation = true
                    };
                    options.ProtocolValidator = new OpenIdConnectProtocolValidator
                    {
                        RequireSub = true,
                        RequireStateValidation = false,
                        NonceLifetime = TimeSpan.FromMinutes(60)
                    };

                    options.DisableTelemetry = true;
                    options.Events = new OpenIdConnectEvents
                    {
                        // Sometimes, problems in the OIDC provider (such as session timeouts)
                        // Redirect the user to the /auth/cb endpoint. ASP.NET Core middleware interprets this by default
                        // as a successful authentication and throws in surprise when it doesn't find an authorization code.
                        // This override ensures that these cases redirect to the root.
                        OnMessageReceived = context =>
                        {
                            var isSpuriousAuthCbRequest =
                                context.Request.Path == options.CallbackPath &&
                                context.Request.Method == "GET" &&
                                !context.Request.Query.ContainsKey("code");

                            if (isSpuriousAuthCbRequest)
                            {
                                context.HandleResponse();
                                context.Response.Redirect("/");
                            }

                            return Task.CompletedTask;
                        },

                        // Sometimes the auth flow fails. The most commonly observed causes for this are
                        // Cookie correlation failures, caused by obscure load balancing stuff.
                        // In these cases, rather than send user to a 500 page, prompt them to re-authenticate.
                        // This is derived from the recommended approach: https://github.com/aspnet/Security/issues/1165
                        OnRemoteFailure = ctx =>
                        {
                            ctx.HandleResponse();
                            return Task.FromException(ctx.Failure);
                        },

                        OnTokenValidated = async ctx =>
                        {
                            ctx.Properties.IsPersistent = true;
                            ctx.Properties.ExpiresUtc = DateTime.UtcNow.Add(overallSessionTimeout);

                            var userInfo = ClaimsPrincipalCurrentUserProvider.MapUserInfoFromPrincipal(ctx.Principal);

                            var signInContext = new SignInContext(ctx.Principal)
                            {
                                UserInfo = userInfo
                            };

                            var signInActions = ctx.HttpContext.RequestServices.GetServices<ISignInAction>();
                            foreach (var a in signInActions)
                            {
                                await a.OnUserSignedIn(signInContext);
                            }

                            ctx.Principal = ClaimsPrincipalCurrentUserProvider.GetPrincipalFromSignInContext(signInContext);

                            if (signInContext.Provider != null)
                            {
                                // For driving legacy views
                                ctx.HttpContext.Session.SetInt32("UKPRN", signInContext.Provider.Ukprn);
                            }
                        }
                    };
                });
        }
    }
}
