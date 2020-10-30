using System.Collections.Generic;
using Dfc.CourseDirectory.FindACourseApi.Helpers;
using Dfc.CourseDirectory.FindACourseApi.Interfaces;
using Dfc.CourseDirectory.FindACourseApi.Services;
using Dfc.CourseDirectory.FindACourseApi.Settings;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;

namespace Dfc.CourseDirectory.FindACourseApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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
                .Configure<SearchServiceSettings>(Configuration.GetSection(nameof(SearchServiceSettings)))
                .AddScoped<ICourseService, CoursesService>()
                .AddSingleton<SearchServiceWrapper>()
                .AddTransient<ISearchServiceSettings>(sp => sp.GetRequiredService<IOptions<SearchServiceSettings>>().Value)
                .AddMediatR(typeof(Startup).Assembly);
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
