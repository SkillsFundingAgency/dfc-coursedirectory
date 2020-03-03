using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Security;
using GovUk.Frontend.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Dfc.CourseDirectory.WebV2
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCourseDirectory(
            this IServiceCollection services,
            IHostingEnvironment environment,
            IConfiguration configuration)
        {
            var thisAssembly = typeof(ServiceCollectionExtensions).Assembly;

            if (!environment.IsTesting())
            {
                services.AddTransient<ICosmosDbQueryDispatcher, CosmosDbQueryDispatcher>();
                services.AddSingleton<Configuration>();

                services.Scan(scan => scan
                    .FromAssembliesOf(typeof(ICosmosDbQuery<>))
                    .AddClasses(classes => classes.AssignableTo(typeof(ICosmosDbQueryHandler<,>)))
                        .AsImplementedInterfaces()
                        .WithTransientLifetime());
            }

            services
                .AddMvc(options =>
                {
                    options.Conventions.Add(new AddFeaturePropertyModelConvention());
                    options.Conventions.Add(new AuthorizeActionModelConvention());

                    options.Filters.Add(new RedirectToProviderSelectionActionFilter());
                    options.Filters.Add(new VerifyApprenticeshipIdActionFilter());
                    options.Filters.Add(new ResourceDoesNotExistExceptionFilter());
                    options.Filters.Add(new DeactivatedProviderErrorActionFilter());

                    options.ModelBinderProviders.Insert(0, new CurrentProviderModelBinderProvider());
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

            services.Scan(scan => scan
                .FromAssembliesOf(typeof(ISqlQuery<>))
                .AddClasses(classes => classes.AssignableTo(typeof(ISqlQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            services.AddSingleton<HostingOptions>();
            services.AddSingleton<IProviderOwnershipCache, ProviderOwnershipCache>();
            services.AddSingleton<IProviderInfoCache, ProviderInfoCache>();
            services.AddGovUkFrontend(new GovUkFrontendAspNetCoreOptions()
            {
                // Avoid import being added to old pages
                AddImportsToHtml = false
            });
            services.AddMediatR(typeof(ServiceCollectionExtensions));
            services.AddScoped<ISqlQueryDispatcher, SqlQueryDispatcher>();
            services.AddScoped<SqlConnection>(_ => new SqlConnection(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<SqlTransaction>(sp =>
            {
                var connection = sp.GetRequiredService<SqlConnection>();
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                var transaction = connection.BeginTransaction(IsolationLevel.Snapshot);

                var marker = sp.GetRequiredService<SqlTransactionMarker>();
                marker.OnTransactionCreated(transaction);

                return transaction;
            });
            services.AddScoped<SqlTransactionMarker>();
            services.AddSingleton<IClock, SystemClock>();
            services.AddSingleton<ICurrentUserProvider, ClaimsPrincipalCurrentUserProvider>();
            services.AddHttpContextAccessor();
            services.TryAddSingleton<IFeatureFlagProvider, ConfigurationFeatureFlagProvider>();

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

                            var helper = ctx.HttpContext.RequestServices.GetRequiredService<DfeUserInfoHelper>();
                            await helper.AppendAdditionalClaims(ctx.Principal);
                        }
                    };
                });
        }
    }
}
