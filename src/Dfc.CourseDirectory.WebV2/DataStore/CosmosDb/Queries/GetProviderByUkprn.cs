using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries
{
    public class GetProviderByUkprn : ICosmosDbQuery<Provider>
    {
        public int Ukprn { get; set; }
    }
}
