using Dfc.CourseDirectory.Common.Settings;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.AuthService;
using Dfc.CourseDirectory.Services.BaseDataAccess;
using Dfc.CourseDirectory.Services.BulkUploadService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.CourseTextService;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces.AuthService;
using Dfc.CourseDirectory.Services.Interfaces.BaseDataAccess;
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
using Dfc.CourseDirectory.Web.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;

namespace Dfc.CourseDirectory.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IAuthService AuthService;
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _env;
        public Startup(IHostingEnvironment env, ILoggerFactory logFactory)
        {
            _env = env;
            _logger = logFactory.CreateLogger<Startup>();
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
            services.AddSingleton(Configuration);
            services.AddApplicationInsightsTelemetry(Configuration);

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

            services.Configure<BaseDataAccessSettings>(Configuration.GetSection(nameof(BaseDataAccessSettings)));
            services.AddScoped<IBaseDataAccess, BaseDataAccess>();

            services.AddSingleton<IAuthService, AuthService>();
            var authSp = services.BuildServiceProvider();
            AuthService = authSp.GetService<IAuthService>();
            services.AddScoped<IAuthService, AuthService>();
            
            services.Configure<GovukPhaseBannerSettings>(Configuration.GetSection(nameof(GovukPhaseBannerSettings)));
            services.AddScoped<IGovukPhaseBannerService, GovukPhaseBannerService>();

            services.AddTransient((provider) => new HttpClient());

            services.Configure<LarsSearchSettings>(Configuration.GetSection(nameof(LarsSearchSettings)));
            services.AddScoped<ILarsSearchService, LarsSearchService>();

            services.Configure<PostCodeSearchSettings>(Configuration.GetSection(nameof(PostCodeSearchSettings)));
            services.AddScoped<IPostCodeSearchService, PostCodeSearchService>();

            services.AddScoped<IPostCodeSearchHelper, PostCodeSearchHelper>();


            services.AddScoped<ILarsSearchHelper, LarsSearchHelper>();
            services.AddScoped<IPaginationHelper, PaginationHelper>();

            services.AddScoped<IVenueSearchHelper, VenueSearchHelper>();

            services.Configure<ProviderServiceSettings>(Configuration.GetSection(nameof(ProviderServiceSettings)));
            services.AddScoped<IProviderService, ProviderService>();
            services.AddScoped<IProviderSearchHelper, ProviderSearchHelper>();

            services.Configure<VenueServiceSettings>(Configuration.GetSection(nameof(VenueServiceSettings)));
            services.AddScoped<IVenueService, VenueService>();

            services.Configure<CourseServiceSettings>(Configuration.GetSection(nameof(CourseServiceSettings)));
            services.AddScoped<ICourseService, CourseService>();

            services.Configure<CourseTextServiceSettings>(Configuration.GetSection(nameof(CourseTextServiceSettings)));
            services.AddScoped<ICourseTextService, CourseTextService>();

            services.Configure<OnspdSearchSettings>(Configuration.GetSection(nameof(OnspdSearchSettings)));
            services.AddScoped<IOnspdService, OnspdService>();
            services.AddScoped<IOnspdSearchHelper, OnspdSearchHelper>();
            services.AddScoped<IUserHelper, UserHelper>();
            services.AddScoped<IBulkUploadService, BulkUploadService>();

            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));


            services.AddIdentity<User, IdentityRole>(options => {
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;

                //Password rules
                options.Password.RequireNonAlphanumeric = false;

            }).AddEntityFrameworkStores<ApplicationDbContext>()
               .AddDefaultTokenProviders();

            services.AddScoped<SignInManager<User>, SignInManager<User>>();
            services.AddSingleton<IEmailSender, EmailSender>();

            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.FromMinutes(1);
            });
            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.LogoutPath = "/Home";
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/Login";
                options.ReturnUrlParameter = "/Home";
                options.SlidingExpiration = true;
            });
            services.AddMvc(options =>
            {


            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddSessionStateTempDataProvider();


            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Developer"));
                options.AddPolicy("ElevatedUserRole", policy => policy.RequireRole("Developer", "Helpdesk"));
                options.AddPolicy("SuperUser", policy => policy.RequireRole("Developer", "Helpdesk", "Provider Superuser"));
                options.AddPolicy("Helpdesk", policy => policy.RequireRole("Helpdesk"));
                options.AddPolicy("Provider", policy => policy.RequireRole("Provider User", "Provider Superuser"));
            });
            services.AddDistributedMemoryCache();

            services.Configure<FormOptions>(x => x.ValueCountLimit = 2048);

            services.AddResponseCaching();
            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;

            });



            #region DFE Sign-in code

            //    //Auth Code
            //    //--------------------------------------
            //    var cookieSecurePolicy = _env.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
            //    services.AddAntiforgery(options => {
            //        options.Cookie.SecurePolicy = cookieSecurePolicy;
            //    });
            //    services.AddAuthentication(options =>
            //    {
            //        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;

            //    }).AddCookie(options =>
            //    {
            //        options.ExpireTimeSpan = TimeSpan.FromHours(6);
            //        options.Events = new CookieAuthenticationEvents
            //        {

            //            // refer to
            //            //  https://github.com/mderriey/TokenRenewal
            //            //  https://stackoverflow.com/questions/40032851/how-to-handle-expired-access-token-in-asp-net-core-using-refresh-token-with-open
            //            // for more details

            //            // this event is fired everytime the cookie has been validated by the cookie middleware,
            //            // so basically during every authenticated request
            //            // the decryption of the cookie has already happened so we have access to the user claims
            //            // and cookie properties - expiration, etc..
            //            OnValidatePrincipal = async x =>
            //            {
            //                // since our cookie lifetime is based on the access token one,
            //                // check if we're more than halfway of the cookie lifetime
            //                // assume a timeout of 20 minutes.
            //                var timeElapsed = DateTimeOffset.UtcNow.Subtract(x.Properties.IssuedUtc.Value);

            //                if (timeElapsed > TimeSpan.FromMinutes(19.5))
            //                {
            //                    var identity = (ClaimsIdentity)x.Principal.Identity;
            //                    var accessTokenClaim = identity.FindFirst("access_token");
            //                    var refreshTokenClaim = identity.FindFirst("refresh_token");

            //                    // if we have to refresh, grab the refresh token from the claims, and request
            //                    // new access token and refresh token
            //                    var refreshToken = refreshTokenClaim.Value;

            //                    var clientId = Configuration.GetSection("DFESignInSettings:ClientID");
            //                    const string envKeyClientSecret = "DFESignInSettings:ClientSecret";
            //                    var clientSecret = Configuration.GetSection(nameof(envKeyClientSecret));
            //                    if (string.IsNullOrWhiteSpace(clientSecret.ToString()))
            //                    {
            //                        throw new Exception("Missing environment variable " + envKeyClientSecret + " - get this from the DfE Sign-in team.");
            //                    }
            //                    var tokenEndpoint = Configuration.GetSection("DFESignInSettings:TokenEndpoint").Value;

            //                    var client = new TokenClient(tokenEndpoint, clientId.ToString(), clientSecret.ToString());
            //                    var response = await client.RequestRefreshTokenAsync(refreshToken, new { client_secret = clientSecret });

            //                    if (!response.IsError)
            //                    {
            //                        // everything went right, remove old tokens and add new ones
            //                        identity.RemoveClaim(accessTokenClaim);
            //                        identity.RemoveClaim(refreshTokenClaim);

            //                        identity.AddClaims(new[]
            //                        {
            //                            new Claim("access_token", response.AccessToken),
            //                                new Claim("refresh_token", response.RefreshToken)
            //                        });

            //                        // indicate to the cookie middleware to renew the session cookie
            //                        // the new lifetime will be the same as the old one, so the alignment
            //                        // between cookie and access token is preserved
            //                        x.ShouldRenew = true;
            //                    }
            //                    else
            //                    {
            //                        // could not refresh - log the user out
            //                        _logger.LogWarning("Token refresh failed with message: " + response.ErrorDescription);
            //                        x.RejectPrincipal();
            //                    }
            //                }
            //            }
            //        };
            //    }).AddOpenIdConnect(options =>
            //    {
            //        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //        options.MetadataAddress = Configuration.GetSection("DFESignInSettings:MetadataAddress").Value;
            //        options.RequireHttpsMetadata = false;

            //        options.ClientId = Configuration.GetSection("DFESignInSettings:ClientID").Value;
            //        const string envKeyClientSecret = "DFESignInSettings:ClientSecret";
            //        var clientSecret = Configuration.GetSection(envKeyClientSecret).Value;
            //        if (string.IsNullOrWhiteSpace(clientSecret.ToString()))
            //        {
            //            throw new Exception("Missing environment variable " + envKeyClientSecret + " - get this from the DfE Sign-in team.");
            //        }

            //        options.ClientSecret = clientSecret.ToString();
            //        options.ResponseType = OpenIdConnectResponseType.Code;
            //        options.GetClaimsFromUserInfoEndpoint = true;

            //        // using this property would align the expiration of the cookie
            //        // with the expiration of the identity token
            //        // UseTokenLifetime = true;

            //        options.Scope.Clear();
            //        options.Scope.Add("openid");
            //        options.Scope.Add("email");
            //        options.Scope.Add("profile");

            //        options.Scope.Add("offline_access");

            //        options.SaveTokens = true;
            //        options.CallbackPath = new PathString(Configuration.GetSection("DFESignInSettings:CallbackPath").Value);
            //        options.SignedOutCallbackPath = new PathString(Configuration.GetSection("DFESignInSettings:SignedOutCallbackPath").Value);
            //        options.SecurityTokenValidator = new JwtSecurityTokenHandler
            //        {
            //            InboundClaimTypeMap = new Dictionary<string, string>(),
            //            TokenLifetimeInMinutes = 20,
            //            SetDefaultTimesOnTokenCreation = true,
            //        };
            //        options.ProtocolValidator = new OpenIdConnectProtocolValidator
            //        {
            //            RequireSub = true,
            //            RequireStateValidation = false,
            //            NonceLifetime = TimeSpan.FromMinutes(15)
            //        };

            //        options.DisableTelemetry = true;
            //        options.Events = new OpenIdConnectEvents
            //        {
            //            // Sometimes, problems in the OIDC provider (such as session timeouts)
            //            // Redirect the user to the /auth/cb endpoint. ASP.NET Core middleware interprets this by default
            //            // as a successful authentication and throws in surprise when it doesn't find an authorization code.
            //            // This override ensures that these cases redirect to the root.
            //            OnMessageReceived = context =>
            //            {
            //                var isSpuriousAuthCbRequest =
            //                    context.Request.Path == options.CallbackPath &&
            //                    context.Request.Method == "GET" &&
            //                    !context.Request.Query.ContainsKey("code");

            //                if (isSpuriousAuthCbRequest)
            //                {
            //                    context.HandleResponse();
            //                    context.Response.StatusCode = 302;
            //                    context.Response.Headers["Location"] = "/";
            //                }

            //                return Task.CompletedTask;
            //            },

            //            // Sometimes the auth flow fails. The most commonly observed causes for this are
            //            // Cookie correlation failures, caused by obscure load balancing stuff.
            //            // In these cases, rather than send user to a 500 page, prompt them to re-authenticate.
            //            // This is derived from the recommended approach: https://github.com/aspnet/Security/issues/1165
            //            OnRemoteFailure = ctx =>
            //            {
            //                ctx.Response.Redirect("/");
            //                ctx.HandleResponse();
            //                return Task.FromResult(0);
            //            },

            //            OnRedirectToIdentityProvider = context =>
            //            {
            //                context.ProtocolMessage.Prompt = "consent";
            //                return Task.CompletedTask;
            //            },

            //            // that event is called after the OIDC middleware received the authorisation code,
            //            // redeemed it for an access token and a refresh token,
            //            // and validated the identity token
            //            OnTokenValidated = x =>
            //            {
            //                var identity = (ClaimsIdentity)x.Principal.Identity;

            //                string email = identity.Claims.Where(c => c.Type == "email")
            //                .Select(c => c.Value).SingleOrDefault();

            //                AuthUserDetails details = AuthService.GetDetailsByEmail(email);

            //                // store both access and refresh token in the claims - hence in the cookie

            //                identity.AddClaims(new[]
            //                {
            //                        new Claim("access_token", x.TokenEndpointResponse.AccessToken),
            //                        new Claim("refresh_token", x.TokenEndpointResponse.RefreshToken),
            //                        new Claim("UKPRN", details.UKPRN),
            //                        new Claim("user_id", details.UserId.ToString()),
            //                        new Claim("role_id", details.RoleId.ToString()),
            //                        new Claim(ClaimTypes.Role, details.RoleName)

            //                });

            //                // so that we don't issue a session cookie but one with a fixed expiration
            //                x.Properties.IsPersistent = true;




            //                return Task.CompletedTask;
            //            }
            //        };
            //    });

            //    //--------------------------------------
            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
             IServiceProvider serviceProvider)
        {
            loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Debug);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStatusCodePages(async context => {
                var response = context.HttpContext.Response;

                if (response.StatusCode == (int)HttpStatusCode.Unauthorized ||
                    response.StatusCode == (int)HttpStatusCode.Forbidden)
                    response.Redirect("/Home");
            });
            app.UseHttpsRedirection();
            app.UseStaticFiles();
           // app.UseCookiePolicy();
            app.UseSession();
            app.UseAuthentication();
            // hotfix
            // workaround for bug in DfE sign in
            // which appends a trailing slash
            //app.UseRewriter(new RewriteOptions()
            //    .AddRedirect("^auth/cb/?.*", "auth/cb"));

            //Preventing ClickJacking Attacks
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Xss-Protection", "1");

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