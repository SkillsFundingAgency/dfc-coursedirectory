using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class GetProviderByUkprn : ICosmosDbQuery<Provider>
    {
        public int Ukprn { get; set; }
    }
}
