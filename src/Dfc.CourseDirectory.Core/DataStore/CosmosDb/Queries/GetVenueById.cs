using System;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class GetVenueById : ICosmosDbQuery<Venue>
    {
        public Guid VenueId { get; set; }
    }
}
