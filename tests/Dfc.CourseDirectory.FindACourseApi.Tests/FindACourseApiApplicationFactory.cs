using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Dfc.CourseDirectory.FindACourseApi.Tests
{
    [CollectionDefinition("FindACourseApi")]
    public class FindACourseApiCollection : ICollectionFixture<FindACourseApiApplicationFactory>
    {
    }

    public class FindACourseApiApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override IWebHostBuilder CreateWebHostBuilder() => base.CreateWebHostBuilder()
            .UseEnvironment("Testing");
    }
}
