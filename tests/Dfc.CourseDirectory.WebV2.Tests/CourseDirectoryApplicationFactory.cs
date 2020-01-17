using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class CourseDirectoryApplicationFactory : WebApplicationFactory<Startup>
    {
        public Mock<IProviderInfoCache> ProviderInfoCache { get; } = new Mock<IProviderInfoCache>();
        public Mock<IProviderOwnershipCache> ProviderOwnershipCache { get; } = new Mock<IProviderOwnershipCache>();

        public void ResetMocks()
        {
            ProviderInfoCache.Reset();
            ProviderOwnershipCache.Reset();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseContentRoot(".");

        protected override IWebHostBuilder CreateWebHostBuilder() => WebHost
            .CreateDefaultBuilder()
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {
                services.AddSingleton(ProviderInfoCache.Object);
                services.AddSingleton(ProviderOwnershipCache.Object);
            })
            .UseStartup<Startup>();
    }
}
