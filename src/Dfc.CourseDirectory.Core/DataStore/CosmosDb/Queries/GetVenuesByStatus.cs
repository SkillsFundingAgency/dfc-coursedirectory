using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class GetVenuesByStatus : ICosmosDbQuery<IReadOnlyCollection<Venue>>
    {
        public VenueStatus Status { get; set; }
    }
}
