using System;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.Models;
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
        public Mock<ICosmosDbQueryDispatcher> CosmosDbQueryDispatcher { get; } = new Mock<ICosmosDbQueryDispatcher>();

        public Mock<ISearchClient<FindACourseOffering>> FindACourseOfferingSearchClient { get; } = new Mock<ISearchClient<FindACourseOffering>>();

        public Mock<ISearchClient<Lars>> LarsSearchClient { get; } = new Mock<ISearchClient<Lars>>();

        public Mock<IRegionCache> RegionCache { get; } = new Mock<IRegionCache>();

        public Mock<ISqlQueryDispatcher> SqlQueryDispatcher { get; } = new Mock<ISqlQueryDispatcher>();

        public void OnTestStarting()
        {
            ResetMocks();
        }

        protected override IWebHostBuilder CreateWebHostBuilder() => base.CreateWebHostBuilder()
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {
                services.AddSingleton(CosmosDbQueryDispatcher.Object);
                services.AddSingleton(FindACourseOfferingSearchClient.Object);
                services.AddSingleton(LarsSearchClient.Object);
                services.AddSingleton(RegionCache.Object);
                services.AddSingleton(SqlQueryDispatcher.Object);
            });

        private void ResetMocks()
        {
            CosmosDbQueryDispatcher.Reset();
            FindACourseOfferingSearchClient.Reset();
            LarsSearchClient.Reset();
            RegionCache.Reset();
            SqlQueryDispatcher.Reset();

            RegionCache.Setup(c => c.GetAllRegions()).ReturnsAsync(Array.Empty<Region>());
        }
    }
}
