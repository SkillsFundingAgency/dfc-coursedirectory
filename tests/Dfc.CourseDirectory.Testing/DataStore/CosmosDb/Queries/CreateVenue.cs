using System;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.Queries
{
    public class CreateVenue : ICosmosDbQuery<Success>
    {
        public Guid VenueId { get; set; }
        public int ProviderUkprn { get; set; }
        public string VenueName { get; set; }
    }
}
