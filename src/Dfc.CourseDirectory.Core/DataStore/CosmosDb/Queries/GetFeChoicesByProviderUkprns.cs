using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class GetFeChoicesByProviderUkprns : ICosmosDbQuery<IReadOnlyDictionary<int, FeChoice>>
    {
        public IEnumerable<int> ProviderUkprns { get; set; }
    }
}
