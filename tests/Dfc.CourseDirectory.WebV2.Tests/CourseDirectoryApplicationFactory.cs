using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class CourseDirectoryApplicationFactory : WebApplicationFactory<Startup>
    {
        public void ResetMocks()
        {
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseContentRoot(".");

        protected override IWebHostBuilder CreateWebHostBuilder() => WebHost
            .CreateDefaultBuilder()
            .UseEnvironment("Testing")
            .ConfigureAppConfiguration(builder =>
            {
                var fileProvider = new ManifestEmbeddedFileProvider(typeof(CourseDirectoryApplicationFactory).Assembly);

                builder
                    .AddJsonFile(fileProvider, "appsettings.Testing.json", optional: false, reloadOnChange: false)
                    .AddEnvironmentVariables();
            })
            .UseStartup<Startup>();
    }
}
