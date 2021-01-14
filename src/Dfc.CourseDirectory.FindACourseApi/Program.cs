using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.FindACourseApi
{
    public class Program
    {
        public static void Main(string[] args) => CreateWebHostBuilder(args).Build().Run();

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddApplicationInsightsSettings();

                    var environmentName = Environment.GetEnvironmentVariable("EnvironmentSettings__EnvironmentName");

                    if (!string.IsNullOrEmpty(environmentName))
                    {
                        builder.AddJsonFile($"appsettings.Environment.{environmentName}.json", optional: true);
                    }
                })
                .UseStartup<Startup>();
    }
}
