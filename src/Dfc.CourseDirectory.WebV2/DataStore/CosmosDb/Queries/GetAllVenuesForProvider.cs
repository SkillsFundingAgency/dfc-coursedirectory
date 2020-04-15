using System.Collections.Generic;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries
{
    public class GetAllVenuesForProvider : ICosmosDbQuery<IReadOnlyCollection<Venue>>
    {
        public int ProviderUkprn { get; set; }
    }
}
