using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

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
            .UseStartup<Startup>();
    }
}
