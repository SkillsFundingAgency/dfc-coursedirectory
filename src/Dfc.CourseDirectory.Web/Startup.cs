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

            services.Configure<VenueSearchSettings>(Configuration.GetSection(nameof(VenueSearchSettings)));
            services.AddScoped<IVenueSearchService, VenueSearchService>();
            services.AddScoped<IVenueSearchHelper, VenueSearchHelper>();

            services.Configure<ProviderSearchSettings>(Configuration.GetSection(nameof(ProviderSearchSettings)));
            services.AddScoped<IProviderSearchService, ProviderSearchService>();
            services.AddScoped<IProviderSearchHelper, ProviderSearchHelper>();

            services.Configure<ProviderAddSettings>(Configuration.GetSection(nameof(ProviderAddSettings)));
            services.AddScoped<IProviderAddService, ProviderAddService>();

            services.Configure<VenueAddSettings>(Configuration.GetSection(nameof(VenueAddSettings)));
            services.AddScoped<IVenueAddService, VenueAddService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddSessionStateTempDataProvider();

            services.AddDistributedMemoryCache();
            services.AddSession(options => {
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
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

            app.UseHttpsRedirection();
            app.UseStaticFiles();
           // app.UseCookiePolicy();
            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "onboardprovider",
                    template: "{controller=ProviderSearch}/{action=OnBoardProvider}/{id?}");  //"restartsitefinity/{controller}/{action}", new { controller = "AdminPanel", action = "RestartSitefinity", id = string.Empty });
            });
        }
    }
}