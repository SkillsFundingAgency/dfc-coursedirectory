using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
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
        public Mock<ISearchClient<FindACourseOffering>> FindACourseOfferingSearchClient { get; } = new Mock<ISearchClient<FindACourseOffering>>();

        public Mock<ISearchClient<Onspd>> OnspdSearchClient { get; } = new Mock<ISearchClient<Onspd>>();

        public Mock<ISearchClient<Lars>> LarsSearchClient { get; } = new Mock<ISearchClient<Lars>>();

        public Mock<ICosmosDbQueryDispatcher> CosmosDbQueryDispatcher { get; } = new Mock<ICosmosDbQueryDispatcher>();

        public void OnTestStarting()
        {
            ResetMocks();
        }

        protected override IWebHostBuilder CreateWebHostBuilder() => base.CreateWebHostBuilder()
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {
                services.AddSingleton(FindACourseOfferingSearchClient.Object);
                services.AddSingleton(OnspdSearchClient.Object);
                services.AddSingleton(LarsSearchClient.Object);
                services.AddSingleton(CosmosDbQueryDispatcher.Object);
            });

        private void ResetMocks()
        {
            FindACourseOfferingSearchClient.Reset();
            OnspdSearchClient.Reset();
            LarsSearchClient.Reset();
            CosmosDbQueryDispatcher.Reset();
        }
    }
}
