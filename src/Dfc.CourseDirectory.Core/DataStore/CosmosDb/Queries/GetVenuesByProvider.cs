using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class GetVenuesByProvider : ICosmosDbQuery<IReadOnlyCollection<Venue>>
    {
        public int ProviderUkprn { get; set; }
    }
}
