using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Helpers;
using Dfc.CourseDirectory.WebV2.Helpers.Interfaces;
using Dfc.CourseDirectory.WebV2.LoqateAddressSearch;
using Dfc.CourseDirectory.WebV2.ModelBinding;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Dfc.CourseDirectory.WebV2.Security;
using Dfc.CourseDirectory.WebV2.Services;
using Dfc.CourseDirectory.WebV2.Services.Interfaces;
using Dfc.CourseDirectory.WebV2.TagHelpers;
using GovUk.Frontend.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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

                    options.Filters.Add(new ProviderContextResourceFilter());
                    options.Filters.Add(new RedirectToProviderSelectionActionFilter());
                    options.Filters.Add(new VerifyApprenticeshipIdActionFilter());
                    options.Filters.Add(new DeactivatedProviderErrorActionFilter());
                    options.Filters.Add(new NotAuthorizedExceptionFilter());
                    options.Filters.Add(new ErrorExceptionFilter());
                    options.Filters.Add(new LocalUrlActionFilter());
                    options.Filters.Add(new MptxResourceFilter());
                    options.Filters.Add(new ContentSecurityPolicyActionFilter());
                    options.Filters.Add(new MptxControllerActionFilter());

                    options.ModelBinderProviders.Insert(0, new ProviderContextModelBinderProvider());
                    options.ModelBinderProviders.Insert(0, new MptxInstanceContextModelBinderProvider());
                    options.ModelBinderProviders.Insert(0, new MultiValueEnumModelBinderProvider());
                    options.ModelBinderProviders.Insert(0, new StandardOrFrameworkModelBinderProvider());
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
                    AuthorizationPolicyNames.ApprenticeshipQA,
                    policy => policy.RequireRole(RoleNames.Developer, RoleNames.Helpdesk));
            });
            
            // SignInActions - order here is the order they're executed in
            services.AddTransient<ISignInAction, DfeUserInfoHelper>();
            services.AddTransient<ISignInAction, EnsureProviderExists>();
            services.AddTransient<ISignInAction, SignInTracker>();
            services.AddTransient<ISignInAction, EnsureApprenticeshipQAStatusSetSignInAction>();
            services.AddTransient<ISignInAction, SyncUserProviderSignInAction>();

            services.AddSqlDataStore(configuration.GetConnectionString("DefaultConnection"));

            if (!environment.IsTesting())
            {
                services.AddCosmosDbDataStore(
                    endpoint: new Uri(configuration["CosmosDbSettings:EndpointUri"]),
                    key: configuration["CosmosDbSettings:PrimaryKey"]);
            }
			
            // HostedService to execute startup tasks.
            // N.B. it's important this is the first HostedService to run; it may set up dependencies for other services.
            services.Insert(
                0,
                new ServiceDescriptor(typeof(IHostedService), typeof(RunStartupTasksHostedService),
                ServiceLifetime.Transient));

            services.AddSingleton<HostingOptions>();
            services.AddSingleton<IProviderOwnershipCache, ProviderOwnershipCache>();
            services.AddSingleton<IProviderInfoCache, ProviderInfoCache>();
            services.AddGovUkFrontend(new GovUkFrontendAspNetCoreOptions()
            {
                // Avoid import being added to old pages
                AddImportsToHtml = false
            });
            services.AddMediatR(typeof(ServiceCollectionExtensions));
            services.AddScoped<IClock, FrozenSystemClock>();
            services.AddSingleton<ICurrentUserProvider, ClaimsPrincipalCurrentUserProvider>();
            services.AddHttpContextAccessor();
            services.TryAddSingleton<IFeatureFlagProvider, ConfigurationFeatureFlagProvider>();
            services.AddScoped<SignInTracker>();
            services.AddBehaviors();
            services.AddSingleton<IStandardsAndFrameworksCache, StandardsAndFrameworksCache>();
            services.AddSingleton<MptxInstanceContextProvider>();
            services.AddMptxInstanceContext();
            services.AddSingleton<IMptxStateProvider, SessionMptxStateProvider>();
            services.AddSingleton<MptxInstanceContextFactory>();
            services.AddSingleton<IProviderContextProvider, ProviderContextProvider>();
            services.AddSingleton(new LoqateAddressSearch.Options() { Key = configuration["PostCodeSearchSettings:Key"] });
            services.AddSingleton<IAddressSearchService, AddressSearchService>();

            services.AddTransient<IUkrlpSyncHelper, UkrlpSyncHelper>();
            services.AddTransient<IUkrlpWcfService, UkrlpWcfService>();
            services.AddTransient<MptxManager>();
            services.AddTransient<Features.NewApprenticeshipProvider.FlowModelInitializer>();
            services.AddTransient<ITagHelperComponent, AppendProviderContextTagHelperComponent>();
            services.AddTransient<ITagHelperComponent, AppendMptxInstanceTagHelperComponent>();
            services.AddTransient<Features.ApprenticeshipQA.ProviderAssessment.FlowModelInitializer>();
            services.AddTransient<Features.ApprenticeshipQA.ApprenticeshipAssessment.FlowModelInitializer>();

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
                    options.Scope.Add("organisation");
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
