using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
                .ConfigureLogging((context,
                builder) =>
                {
                    builder.AddConfiguration(context.Configuration.GetSection("Logging"));
                    var ikey = context.Configuration.GetSection("APPINSIGHTS_INSTRUMENTATIONKEY");
                    builder.AddApplicationInsights(ikey.Value);

                }
        );
    }
}