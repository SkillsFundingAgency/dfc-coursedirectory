using System.Net.Http;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.FindACourseApi.Tests
{
    [Collection("FindACourseApi")]
    public abstract class TestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        public TestBase(FindACourseApiApplicationFactory factory)
        {
            Factory = factory;
            HttpClient = factory.CreateClient();
            Factory.OnTestStarting();
        }

        public HttpClient HttpClient { get; }

        public FindACourseApiApplicationFactory Factory { get; }

        protected Mock<ISearchClient<FindACourseOffering>> FindACourseOfferingSearchClient => Factory.FindACourseOfferingSearchClient;

        protected Mock<ISearchClient<Onspd>> OnspdSearchClient => Factory.OnspdSearchClient;

        protected Mock<ISearchClient<Lars>> LarsSearchClient => Factory.LarsSearchClient;

        protected Mock<ICosmosDbQueryDispatcher> CosmosDbQueryDispatcher => Factory.CosmosDbQueryDispatcher;

        protected Mock<ISqlQueryDispatcher> SqlQueryDispatcher => Factory.SqlQueryDispatcher;
    }
}
