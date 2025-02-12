using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Dfc.CourseDirectory.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddConfiguration(context.Configuration.GetSection("Logging"));
                    var appInsightsConnectionString = context.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
                    builder.AddApplicationInsights(
                        configureTelemetryConfiguration: (config) =>
                            config.ConnectionString = appInsightsConnectionString,
                            configureApplicationInsightsLoggerOptions: (options) => { }
                        );
                })
                .ConfigureAppConfiguration(builder =>
                {
                    var environmentName = Environment.GetEnvironmentVariable("EnvironmentSettings__EnvironmentName");

                    if (!string.IsNullOrEmpty(environmentName))
                    {
                        builder.AddJsonFile($"appsettings.Environment.{environmentName}.json", optional: true);
                    }
                });
    }
}
