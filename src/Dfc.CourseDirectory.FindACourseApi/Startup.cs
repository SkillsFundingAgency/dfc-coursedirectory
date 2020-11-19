using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Search.AzureSearch;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.FindACourseApi.Interfaces;
using Dfc.CourseDirectory.FindACourseApi.Services;
using Dfc.CourseDirectory.FindACourseApi.Settings;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;

namespace Dfc.CourseDirectory.FindACourseApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddNewtonsoftJson(jsonOptions => jsonOptions.SerializerSettings.Converters.Insert(0, new StringEnumConverter()));

            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Find a Course API", Version = "v1" });
            });

            services
                .Configure<CourseServiceSettings>(Configuration.GetSection(nameof(CourseServiceSettings)))
                .AddScoped<ICourseService, CoursesService>()
                .AddMediatR(typeof(Startup).Assembly);

            if (Environment.EnvironmentName != "Testing")
            {
                services.AddAzureSearchClient<Onspd>(
                    new Uri(Configuration["AzureSearchUrl"]),
                    Configuration["AzureSearchQueryKey"],
                    indexName: "onspd");

                services.AddAzureSearchClient<Course>(
                    new Uri(Configuration["AzureSearchUrl"]),
                    Configuration["AzureSearchQueryKey"],
                    indexName: "course");
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseSwagger(c =>
            {
                c.SerializeAsV2 = true;

                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    swaggerDoc.Servers = new List<OpenApiServer>
                    {
                        new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" }
                    };
                });
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Find a Course API");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
