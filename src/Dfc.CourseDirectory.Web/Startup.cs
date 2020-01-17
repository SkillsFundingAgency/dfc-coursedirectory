
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Settings;
using Dfc.CourseDirectory.Models.Models.Auth;
using Dfc.CourseDirectory.Models.Models.Environment;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.ApprenticeshipService;
using Dfc.CourseDirectory.Services.AuthService;
using Dfc.CourseDirectory.Services.BaseDataAccess;
using Dfc.CourseDirectory.Services.BlobStorageService;
using Dfc.CourseDirectory.Services.BulkUploadService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.CourseTextService;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.AuthService;
using Dfc.CourseDirectory.Services.Interfaces.BaseDataAccess;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseTextService;
using Dfc.CourseDirectory.Services.Interfaces.OnspdService;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.OnspdService;
using Dfc.CourseDirectory.Services.ProviderService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Areas.Identity.Data;
using Dfc.CourseDirectory.Web.BackgroundWorkers;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.HostedServices;
using Dfc.CourseDirectory.Web.ViewComponents;
using Dfc.CourseDirectory.WebV2;
using IdentityModel.Client;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private readonly ILogger<Startup> _logger;
        private readonly IHostingEnvironment _env;
        //Undefined is only part of these policy until the batch import to update ProviderType is run
        private readonly List<string> _feClaims = new List<string> {"Fe", "Both", "Undefined" };
        private readonly List<string> _apprenticeshipClaims = new List<string> { "Apprenticeship", "Both", "Undefined" };
        public Startup(IHostingEnvironment env, ILogger<Startup> logger, IConfiguration config)
        {
            _env = env;
            _logger = logger;

            var builder = new ConfigurationBuilder()
                .SetBasePath(_env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{_env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddSingleton(Configuration);

            _logger.LogCritical("Logging from ConfigureServices.");
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSingleton<IConfiguration>(Configuration);
            services.Configure<VenueNameComponentSettings>(Configuration.GetSection("AppUISettings:VenueNameComponentSettings"));
            services.Configure<CourseForComponentSettings>(Configuration.GetSection("AppUISettings:CourseForComponentSettings"));
            services.Configure<EntryRequirementsComponentSettings>(Configuration.GetSection("AppUISettings:EntryRequirementsComponentSettings"));
            services.Configure<WhatWillLearnComponentSettings>(Configuration.GetSection("AppUISettings:WhatWillLearnComponentSettings"));
            services.Configure<HowYouWillLearnComponentSettings>(Configuration.GetSection("AppUISettings:HowYouWillLearnComponentSettings"));
            services.Configure<WhatYouNeedComponentSettings>(Configuration.GetSection("AppUISettings:WhatYouNeedComponentSettings"));
            services.Configure<HowAssessedComponentSettings>(Configuration.GetSection("AppUISettings:HowAssessedComponentSettings"));
            services.Configure<WhereNextComponentSettings>(Configuration.GetSection("AppUISettings:WhereNextComponentSettings"));

            services.AddOptions();

            services.Configure<BaseDataAccessSettings>(options =>
            {
                options.ConnectionString = Configuration.GetConnectionString("DefaultConnection");
            });
            services.AddScoped<IBaseDataAccess, BaseDataAccess>();

            services.Configure<ProviderServiceSettings>(Configuration.GetSection(nameof(ProviderServiceSettings)));
            services.AddScoped<IProviderService, ProviderService>();
            services.AddScoped<IProviderSearchHelper, ProviderSearchHelper>();

            services.AddTransient((provider) => new HttpClient());

            services.AddScoped<IAuthService, AuthService>();
            services.Configure<GovukPhaseBannerSettings>(Configuration.GetSection(nameof(GovukPhaseBannerSettings)));
            services.Configure<ApprenticeshipSettings>(Configuration.GetSection(nameof(ApprenticeshipSettings)));
            services.AddScoped<IGovukPhaseBannerService, GovukPhaseBannerService>();


            services.Configure<LarsSearchSettings>(Configuration.GetSection(nameof(LarsSearchSettings)));
            services.AddScoped<ILarsSearchService, LarsSearchService>();

            services.Configure<PostCodeSearchSettings>(Configuration.GetSection(nameof(PostCodeSearchSettings)));
            services.AddScoped<IPostCodeSearchService, PostCodeSearchService>();
            services.AddScoped<ILarsSearchHelper, LarsSearchHelper>();
            services.AddScoped<IPaginationHelper, PaginationHelper>();


            services.AddScoped<IVenueSearchHelper, VenueSearchHelper>();
            services.Configure<VenueServiceSettings>(Configuration.GetSection(nameof(VenueServiceSettings)));
            services.AddScoped<IVenueService, VenueService>();

            services.Configure<CourseServiceSettings>(Configuration.GetSection(nameof(CourseServiceSettings)));
            services.Configure<FindACourseServiceSettings>(Configuration.GetSection(nameof(FindACourseServiceSettings)));
            services.AddScoped<ICourseService, CourseService>();

            services.Configure<CourseTextServiceSettings>(Configuration.GetSection(nameof(CourseTextServiceSettings)));
            services.AddScoped<ICourseTextService, CourseTextService>();

            services.Configure<OnspdSearchSettings>(Configuration.GetSection(nameof(OnspdSearchSettings)));
            services.AddScoped<IOnspdService, OnspdService>();
            services.AddScoped<IOnspdSearchHelper, OnspdSearchHelper>();
            services.AddScoped<IUserHelper, UserHelper>();
            services.AddScoped<ICSVHelper, CSVHelper>();
            services.AddScoped<ICourseProvisionHelper, CourseProvisionHelper>();
            services.Configure<ApprenticeshipServiceSettings>(Configuration.GetSection(nameof(ApprenticeshipServiceSettings)));
            services.AddScoped<IApprenticeshipService, ApprenticeshipService>();

            services.AddScoped<IBulkUploadService, BulkUploadService>();
            services.AddScoped<IApprenticeshipBulkUploadService, ApprenticeshipBulkUploadService>();
            services.Configure<BlobStorageSettings>(Configuration.GetSection(nameof(BlobStorageSettings)));
            services.AddScoped<IBlobStorageService, BlobStorageService>();
            services.Configure<EnvironmentSettings>(Configuration.GetSection(nameof(EnvironmentSettings)));
            services.AddScoped<IEnvironmentHelper, EnvironmentHelper>();
            services.AddScoped<IApprenticeshipProvisionHelper, ApprenticeshipProvisionHelper>();
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddCourseDirectory(_env);

            services.AddMvc(options =>
            {
                options.Filters.Add(new DeactivatedProviderErrorActionFilter());
                options.Filters.Add(new RedirectOnMissingUKPRNActionFilter());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddSessionStateTempDataProvider();


            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Developer"));
                options.AddPolicy("ElevatedUserRole", policy => policy.RequireRole("Developer", "Helpdesk"));
                options.AddPolicy("SuperUser", policy => policy.RequireRole("Developer", "Helpdesk", "Provider Superuser"));
                options.AddPolicy("Helpdesk", policy => policy.RequireRole("Helpdesk"));
                options.AddPolicy("ProviderSuperUser", policy => policy.RequireRole("Provider Superuser"));
                options.AddPolicy("Provider", policy => policy.RequireRole("Provider User", "Provider Superuser"));
                options.AddPolicy("Apprenticeship", policy =>
                    policy.RequireAssertion(x => (!x.User.IsInRole("Provider Superuser") && !x.User.IsInRole("Provider User")) ||
                                                 x.User.Claims.Any(c => c.Type == "ProviderType" &&
                                                                        _apprenticeshipClaims.Contains(c.Value))));
                options.AddPolicy("Fe", policy =>
                    policy.RequireAssertion(x => (!x.User.IsInRole("Provider Superuser") && !x.User.IsInRole("Provider User")) ||
                                                                             x.User.Claims.Any(c => c.Type == "ProviderType" && 
                                                                                                    _feClaims.Contains(c.Value, StringComparer.OrdinalIgnoreCase))));
            });
            services.AddDistributedMemoryCache();

            services.Configure<FormOptions>(x => x.ValueCountLimit = 2048);

            services.AddResponseCaching();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(40);
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            //TODO
            //services.Configure<GoogleAnalyticsOptions>(options => Configuration.GetSection("GoogleAnalytics").Bind(options));


            services.AddTransient<ITagHelperComponent, GoogleAnalyticsTagHelperComponent>();


            // Register the background worker helper
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.Configure<FormOptions>(x => x.ValueCountLimit = 10000);
            #region DFE Sign-in code

            //Auth Code
            //--------------------------------------
            var overallSessionTimeout = TimeSpan.FromMinutes(90);

            var cookieSecurePolicy = _env.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
            services.AddAntiforgery(options =>
            {
                options.Cookie.SecurePolicy = cookieSecurePolicy;
            });
            services.AddAuthentication(options =>
            {
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;

            }).AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(40);
                options.SlidingExpiration = true;
                options.LogoutPath = "/Auth/Logout";
                options.Events = new CookieAuthenticationEvents
                {

                    // refer to
                    //  https://github.com/mderriey/TokenRenewal
                    //  https://stackoverflow.com/questions/40032851/how-to-handle-expired-access-token-in-asp-net-core-using-refresh-token-with-open
                    // for more details

                    // this event is fired everytime the cookie has been validated by the cookie middleware,
                    // so basically during every authenticated request
                    // the decryption of the cookie has already happened so we have access to the user claims
                    // and cookie properties - expiration, etc..
                    OnValidatePrincipal = async x =>
                    {
                        // since our cookie lifetime is based on the access token one,
                        // check if we're more than halfway of the cookie lifetime
                        // assume a timeout of 20 minutes.
                        var timeElapsed = DateTimeOffset.UtcNow.Subtract(x.Properties.IssuedUtc.Value);

                        if (timeElapsed > TimeSpan.FromMinutes(59.5))
                        {
                            var identity = (ClaimsIdentity)x.Principal.Identity;
                            var accessTokenClaim = identity.FindFirst("access_token");
                            var refreshTokenClaim = identity.FindFirst("refresh_token");

                            if (refreshTokenClaim?.Value == null)
                            {
                                return;
                            }

                            // if we have to refresh, grab the refresh token from the claims, and request
                            // new access token and refresh token
                            var refreshToken = refreshTokenClaim.Value;
                            
                            var clientId = Configuration.GetSection("DFESignInSettings:ClientID").Value;
                            const string envKeyClientSecret = "DFESignInSettings:ClientSecret";
                            var clientSecret = Configuration.GetSection("DFESignInSettings:ClientSecret").Value;
                            if (string.IsNullOrWhiteSpace(clientSecret.ToString()))
                            {
                                throw new Exception("Missing environment variable " + envKeyClientSecret + " - get this from the DfE Sign-in team.");
                            }
                            var tokenEndpoint = Configuration.GetSection("DFESignInSettings:TokenEndpoint").Value;

                            TokenResponse response;
                            using (var client = new HttpClient())
                            {
                                response = await client.RequestRefreshTokenAsync(new RefreshTokenRequest()
                                {
                                    Address = tokenEndpoint,
                                    ClientId = clientId,
                                    ClientSecret = clientSecret,
                                    RefreshToken = refreshToken
                                });
                            }

                            if (!response.IsError)
                            {
                                // everything went right, remove old tokens and add new ones
                                identity.RemoveClaim(accessTokenClaim);
                                identity.RemoveClaim(refreshTokenClaim);

                                identity.AddClaims(new[]
                                {
                                    new Claim("access_token", response.AccessToken),
                                    new Claim("refresh_token", response.RefreshToken)
                                });

                                // indicate to the cookie middleware to renew the session cookie
                                // the new lifetime will be the same as the old one, so the alignment
                                // between cookie and access token is preserved
                                x.ShouldRenew = true;
                            }
                            else
                            {
                                // could not refresh - log the user out
                                _logger.LogWarning("Token refresh failed with message: " + response.ErrorDescription);
                                x.RejectPrincipal();
                            }
                        }
                    }
                };
            }).AddOpenIdConnect(options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.MetadataAddress = Configuration.GetSection("DFESignInSettings:MetadataAddress").Value;
                options.RequireHttpsMetadata = false;

                options.ClientId = Configuration.GetSection("DFESignInSettings:ClientID").Value;
                const string envKeyClientSecret = "DFESignInSettings:ClientSecret";
                var clientSecret = Configuration.GetSection(envKeyClientSecret).Value;
                if (string.IsNullOrWhiteSpace(clientSecret.ToString()))
                {
                    throw new Exception("Missing environment variable " + envKeyClientSecret + " - get this from the DfE Sign-in team.");
                }

                options.ClientSecret = clientSecret.ToString();
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.GetClaimsFromUserInfoEndpoint = true;

                // using this property would align the expiration of the cookie
                // with the expiration of the identity token
                // UseTokenLifetime = true;

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("email");
                options.Scope.Add("profile");
                options.Scope.Add("organisation");
                options.Scope.Add("offline_access");

                // Prompt=consent is required to be issued with a refresh token
                options.Prompt = "consent";

                // When we expire the session, ensure user is prompted to sign in again at DfE Sign In
                options.MaxAge = overallSessionTimeout;

                options.SaveTokens = true;
                options.CallbackPath = new PathString(Configuration.GetSection("DFESignInSettings:CallbackPath").Value);
                options.SignedOutCallbackPath = new PathString(Configuration.GetSection("DFESignInSettings:SignedOutCallbackPath").Value);
                options.SecurityTokenValidator = new JwtSecurityTokenHandler
                {
                    InboundClaimTypeMap = new Dictionary<string, string>(),
                    TokenLifetimeInMinutes = 90,
                    SetDefaultTimesOnTokenCreation = true,
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
                            _logger.LogWarning("Spurious log in attempt received for DFE sign in");

                            context.HandleResponse();
                            context.Response.StatusCode = 302;
                            context.Response.Headers["Location"] = "/";
                        }
                        
                        return Task.CompletedTask;
                    },

                    // Sometimes the auth flow fails. The most commonly observed causes for this are
                    // Cookie correlation failures, caused by obscure load balancing stuff.
                    // In these cases, rather than send user to a 500 page, prompt them to re-authenticate.
                    // This is derived from the recommended approach: https://github.com/aspnet/Security/issues/1165
                    OnRemoteFailure = ctx =>
                    {
                        _logger.LogWarning("Remote failure for DFE-sign in");            
                        ctx.HandleResponse();
                        return Task.FromException(ctx.Failure);
                    },

                    OnRedirectToIdentityProvider = context =>
                    {
                        return Task.CompletedTask;
                    },

                    // that event is called after the OIDC middleware received the authorisation code,
                    // redeemed it for an access token and a refresh token,
                    // and validated the identity token
                    OnTokenValidated = async x =>
                    {
                        _logger.LogMethodEnter();
                        _logger.LogWarning("User has been authorised by DFE");
                        var issuer =  Configuration.GetSection("DFESignInSettings:Issuer").Value;
                        var audience = Configuration.GetSection("DFESignInSettings:Audience").Value;
                        var apiSecret = Configuration.GetSection("DFESignInSettings:APISecret").Value;
                        var apiUri = Configuration.GetSection("DFESignInSettings:APIUri").Value;

                        Throw.IfNull(issuer, nameof(issuer));
                        Throw.IfNull(audience, nameof(audience));
                        Throw.IfNull(apiSecret, nameof(apiSecret));
                        Throw.IfNull(apiUri, nameof(apiUri));

                        var token = new JwtBuilder()
                            .WithAlgorithm(new HMACSHA256Algorithm())
                            .Issuer(issuer)
                            .Audience(audience)
                            .WithSecret(apiSecret)
                            .Build();

                        //Gather user/org details
                        var identity = (ClaimsIdentity)x.Principal.Identity;
                        Organisation organisation;
                        try
                        {
                            organisation = JsonConvert.DeserializeObject<Organisation>(
                            identity.Claims.Where(c => c.Type == "organisation")
                            .Select(c => c.Value).FirstOrDefault());
                        }
                        catch
                        {
                            throw new SystemException("Unable to get organisation details from DFE. Please clear session cookies, or try using private browsing mode.");
                        }

                        DFEClaims userClaims = new DFEClaims
                        {
                            UserId = Guid.Parse(identity.Claims.Where(c => c.Type == "sub").Select(c => c.Value).SingleOrDefault()),
                        };

                        HttpClient client = new HttpClient();
                        client.SetBearerToken(token);
                        var response = await client.GetAsync($"{apiUri}/organisations/{organisation.Id}/users/{userClaims.UserId}");

                        if(response.IsSuccessStatusCode)
                        {
                            var json = response.Content.ReadAsStringAsync().Result;
                            userClaims = JsonConvert.DeserializeObject<DFEClaims>(json);
                            userClaims.RoleName = userClaims.Roles.Select(r => r.Name).FirstOrDefault();
                            userClaims.UKPRN = organisation.UKPRN.HasValue ? organisation.UKPRN.Value.ToString() : string.Empty;
                            userClaims.UserName = identity.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault();
                        }
                        else
                        {
                            throw new SystemException("Could not get Role Type for User");
                        }

                        _logger.LogError("User " + userClaims.UserName + " has been authenticated by DFE");

                        //Course Directory Authorisation
                        try
                        {
                            var authService = x.HttpContext.RequestServices.GetRequiredService<IAuthService>();
                            var providerType = await authService.GetProviderType(UKPRN: userClaims.UKPRN, roleName: userClaims.RoleName);

                            // store both access and refresh token in the claims - hence in the cookie
                            identity.AddClaims(new[]
                            {
                                new Claim("access_token", x.TokenEndpointResponse.AccessToken),
                                new Claim("refresh_token", x.TokenEndpointResponse.RefreshToken),
                                new Claim("UKPRN", userClaims.UKPRN),
                                new Claim("user_id", userClaims.UserId.ToString()),
                                new Claim(ClaimTypes.Role, userClaims.RoleName),
                                new Claim("ProviderType", providerType),
                                new Claim("OrganisationId", organisation.Id.ToString().ToUpper())
                            });

                            _logger.LogWarning("User " + userClaims.UserName + " has been authorised");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Error authorising user", ex);
                            throw new SystemException("Unable to authorise user");
                        }

                        // Check the provider status - a filter will show an error if the provider has been deactivated
                        if (!string.IsNullOrEmpty(userClaims.UKPRN))
                        {
                            var providerService = x.HttpContext.RequestServices.GetRequiredService<IProviderService>();
                            var provider = await providerService.GetProviderByPRNAsync(new Services.ProviderService.ProviderSearchCriteria(userClaims.UKPRN));

                            if (provider.IsSuccess)
                            {
                                identity.AddClaim(new Claim("provider_status", provider.Value.Value.Single().ProviderStatus));
                            }
                            else
                            {
                                throw new Exception("Unable to retrieve provider details.");
                            }
                        }
                         
                        // so that we don't issue a session cookie but one with a fixed expiration
                        x.Properties.IsPersistent = true;

                        x.Properties.ExpiresUtc = DateTime.UtcNow.Add(overallSessionTimeout);
                    }
                };
            });

            //--------------------------------------
            #endregion

        }
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Debug);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                //Uncomment to redirect to live error page
                //app.UseExceptionHandler("/Home/Error");
            }
            else
            {
                app.UseCourseDirectoryErrorHandling();
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseAuthentication();

            //Preventing ClickJacking Attacks
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
                context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Add("Feature-Policy", "accelerometer 'none'; camera 'none'; geolocation 'none'; gyroscope 'none'; magnetometer 'none'; microphone 'none'; payment 'none'; usb 'none'");
                    
                //CSP
                context.Response.Headers.Add("Content-Security-Policy", 
                                                "default-src    'self' " + 
                                                    " https://rainmaker.tiny.cloud/" +
                                                    " https://www.google-analytics.com/" +
                                                    ";" +
                                                "style-src      'self' 'unsafe-inline' "+
                                                    " https://cdn.tiny.cloud/" +                                                    
                                                    " https://www.googletagmanager.com/" +
                                                    " https://tagmanager.google.com/" +
                                                    " https://fonts.googleapis.com/" +
                                                    " https://cloud.tinymce.com/" +
                                                    ";" +
                                                "font-src       'self' data:" +
                                                   " https://fonts.googleapis.com/" +
                                                   " https://fonts.gstatic.com/" +
                                                   " https://cdn.tiny.cloud/" +
                                                   ";" +
                                                "img-src        'self' * data: https://cdn.tiny.cloud/;" +
                                                "script-src     'self' 'unsafe-eval' 'unsafe-inline'  " +
                                                    " https://cloud.tinymce.com/" +
                                                    " https://cdnjs.cloudflare.com/" +
                                                    " https://www.googletagmanager.com/" +
                                                    " https://tagmanager.google.com/" +                                                    
                                                    " https://www.google-analytics.com/" +
                                                    " https://cdn.tiny.cloud/" +
                                                    ";"
                );

                context.Response.GetTypedHeaders().CacheControl =
                  new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                  {
                      NoCache = true,
                      NoStore = true,
                      MustRevalidate = true,
                  };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
                    new string[] { "Pragma: no-cache" };

                await next();
            });


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "onboardprovider",
                    template: "{controller=ProviderSearch}/{action=OnBoardProvider}/{id?}");
            });

        }
    }
}
