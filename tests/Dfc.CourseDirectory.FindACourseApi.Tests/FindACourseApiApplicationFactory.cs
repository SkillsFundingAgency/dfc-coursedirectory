using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.FindACourseApi.Tests
{
    [CollectionDefinition("FindACourseApi")]
    public class FindACourseApiCollection : ICollectionFixture<FindACourseApiApplicationFactory>
    {
    }

    public class FindACourseApiApplicationFactory : WebApplicationFactory<Startup>
    {
        public void OnTestStarting()
        {
            ResetMocks();
        }

        protected override IWebHostBuilder CreateWebHostBuilder() => base.CreateWebHostBuilder()
            .UseEnvironment("Testing");

        private void ResetMocks()
        {
        }
    }
}
