using System;
using System.Threading.Tasks;
using Query = Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.Queries.CreateFramework;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public partial class TestData
    {
        public async Task CreateFramework(
            int frameworkCode,
            int progType,
            int pathwayCode,
            string nasTitle)
        {
            var id = Guid.NewGuid().ToString();

            await _cosmosDbQueryDispatcher.ExecuteQuery(
                new Query()
                {
                    Id = id,
                    FrameworkCode = frameworkCode,
                    ProgType = progType,
                    PathwayCode = pathwayCode,
                    NasTitle = nasTitle
                });
        }
    }
}
