using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class GetFeChoiceForProvider : ICosmosDbQuery<FeChoice>
    {
        public int ProviderUkprn { get; set; }
    }
}
