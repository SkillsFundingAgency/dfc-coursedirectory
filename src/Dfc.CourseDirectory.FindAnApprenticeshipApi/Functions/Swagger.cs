using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Builder;

namespace Dfc.Providerportal.FindAnApprenticeship.Functions
{
    public static class Swagger
    {
        private static readonly IWebHost host = new WebHostBuilder().UseStartup<SwaggerStartup>().Build();

        [FunctionName("Swagger")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "swagger.json")]HttpRequest req)
        {
            var url = req.GetDisplayUrl();

            if (req.Path.HasValue && !string.IsNullOrWhiteSpace(req.Path.Value))
            {
                url = url.Replace(req.Path, string.Empty);
            }

            if (req.QueryString.HasValue && !string.IsNullOrWhiteSpace(req.QueryString.Value))
            {
                url = url.Replace(req.QueryString.Value, string.Empty);
            }

            var swaggerProvider = host.Services.GetRequiredService<ISwaggerProvider>();

            var swagger = swaggerProvider.GetSwagger("v1");

            swagger.Servers.Add(new OpenApiServer { Url = url });

            using (var writer = new StringWriter(new StringBuilder()))
            {
                var jsonWriter = new OpenApiJsonWriter(writer);
                //swagger.SerializeAsV3(jsonWriter);
                swagger.SerializeAsV2(jsonWriter);

                return new JsonResult(JObject.Parse(writer.ToString()));
            }
        }

        private class SwaggerStartup
        {
            public SwaggerStartup(IConfiguration configuration)
            {
                Configuration = configuration;
            }

            public IConfiguration Configuration { get; }

            public void ConfigureServices(IServiceCollection services)
            {
                services
                    .AddMvcCore()
                    .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver())
                    .AddApiExplorer();

                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Course Directory Find An Apprenticeship API", Version = "v1" });
                });
            }

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app)
            {
                app.UseSwagger();

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Course Directory Course API");
                });

                app.UseMvc();
            }
        }
    }
}