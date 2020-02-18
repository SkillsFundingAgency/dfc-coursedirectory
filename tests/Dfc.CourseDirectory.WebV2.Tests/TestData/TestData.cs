using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public partial class TestData
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public TestData(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }
    }
}
