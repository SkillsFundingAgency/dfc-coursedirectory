using System;
using Dfc.ProviderPortal.FindACourse.Helpers;
using Dfc.ProviderPortal.FindACourse.Helpers.Faoc;
using Dfc.ProviderPortal.FindACourse.Interfaces;
using Dfc.ProviderPortal.FindACourse.Interfaces.Faoc;
using Dfc.ProviderPortal.FindACourse.Services;
using Dfc.ProviderPortal.FindACourse.Settings;
using Dfc.ProviderPortal.FindACourse.Settings.Faoc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.Swagger;

namespace Dfc.ProviderPortal.FindACourse.API
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;

        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            _env = env;

            Configuration = //configuration;
                new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{_env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .AddApplicationInsightsSettings()
                .Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddJsonOptions(jsonOptions => jsonOptions.SerializerSettings.Converters.Insert(0, new StringEnumConverter()))
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Find a Course API", Version = "v1" });
            });

            services.Configure<CosmosDbCollectionSettings>(Configuration.GetSection(nameof(CosmosDbCollectionSettings)))
                    .Configure<CosmosDbSettings>(Configuration.GetSection(nameof(CosmosDbSettings)))
                    .Configure<ProviderServiceSettings>(Configuration.GetSection(nameof(ProviderServiceSettings)))
                    .Configure<VenueServiceSettings>(Configuration.GetSection(nameof(VenueServiceSettings)))
                    .Configure<CourseServiceSettings>(Configuration.GetSection(nameof(CourseServiceSettings)))
                    .Configure<SearchServiceSettings>(Configuration.GetSection(nameof(SearchServiceSettings)))
                    .Configure<OnlineCourseSearchServiceSettings>(Configuration.GetSection(nameof(OnlineCourseSearchServiceSettings)))
                    .Configure<QualificationServiceSettings>(Configuration.GetSection(nameof(QualificationServiceSettings)))
                    .AddScoped<ICourseService, CoursesService>()
                    .AddScoped<IOnlineCourseService, OnlineCoursesService>()
                    .AddScoped<ICosmosDbHelper, CosmosDbHelper>()
                    .AddScoped<IProviderServiceWrapper, ProviderServiceWrapper>()
                    .AddScoped<IVenueServiceWrapper, VenueServiceWrapper>()
                    .AddSingleton<SearchServiceWrapper>()
                    .AddSingleton<OnlineCourseSearchServiceWrapper>()
                    .AddTransient<ISearchServiceSettings>(sp => sp.GetRequiredService<IOptions<SearchServiceSettings>>().Value)
                    .AddTransient<IOnlineCourseSearchServiceSettings>(sp => sp.GetRequiredService<IOptions<OnlineCourseSearchServiceSettings>>().Value);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Host = httpReq.Host.Value);
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Find a Course API");
            });

            app.UseMvc();
        }
    }
}