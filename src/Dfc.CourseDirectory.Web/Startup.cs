using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common.Settings;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Services.ProviderService;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Data;
using Microsoft.EntityFrameworkCore;
using Dfc.CourseDirectory.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace Dfc.CourseDirectory.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
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

            services.AddDbContext<ApplicationDbContext>(options =>
              options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));


            services.AddIdentity<DfcCourseDirectoryUser, IdentityRole>(options => {
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
                options.SignIn.RequireConfirmedEmail = true;
            }).AddEntityFrameworkStores<ApplicationDbContext>()
               .AddDefaultTokenProviders();

            services.AddScoped<SignInManager<DfcCourseDirectoryUser>, SignInManager<DfcCourseDirectoryUser>>();

            // using Microsoft.AspNetCore.Identity.UI.Services;
            services.AddSingleton<IEmailSender, EmailSender>();
            services.Configure<AuthMessageSenderOptions>(Configuration.GetSection("SendGrid"));

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddSessionStateTempDataProvider();

            services.AddDistributedMemoryCache();

            services.AddResponseCaching();
            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.HttpOnly = true;
            });
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
            var dbContext = serviceProvider.GetService<ApplicationDbContext>();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
           // app.UseCookiePolicy();
            app.UseSession();
            app.UseAuthentication();
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