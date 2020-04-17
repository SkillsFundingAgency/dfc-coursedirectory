using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Query = Dfc.CourseDirectory.Testing.DataStore.CosmosDb.Queries.CreateFramework;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<Framework> CreateFramework(
            int frameworkCode,
            int progType,
            int pathwayCode,
            string nasTitle)
        {
            var id = Guid.NewGuid();

            await _cosmosDbQueryDispatcher.ExecuteQuery(
                new Query()
                {
                    Id = id,
                    FrameworkCode = frameworkCode,
                    ProgType = progType,
                    PathwayCode = pathwayCode,
                    NasTitle = nasTitle
                });

            return new Framework()
            {
                FrameworkCode = frameworkCode,
                ProgType = progType,
                PathwayCode = pathwayCode,
                NasTitle = nasTitle
            };
        }
    }
}
