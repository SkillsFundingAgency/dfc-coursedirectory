using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
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
        public Mock<ISearchClient<Course>> CourseSearchClient { get; } = new Mock<ISearchClient<Course>>();

        public Mock<ISearchClient<Onspd>> OnspdSearchClient { get; } = new Mock<ISearchClient<Onspd>>();

        public void OnTestStarting()
        {
            ResetMocks();
        }

        protected override IWebHostBuilder CreateWebHostBuilder() => base.CreateWebHostBuilder()
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {
                services.AddSingleton<ISearchClient<Course>>(CourseSearchClient.Object);
                services.AddSingleton<ISearchClient<Onspd>>(OnspdSearchClient.Object);
            });

        private void ResetMocks()
        {
            CourseSearchClient.Reset();
            OnspdSearchClient.Reset();
        }
    }
}
