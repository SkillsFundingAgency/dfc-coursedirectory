using System;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.Queries
{
    public class CreateVenue : ICosmosDbQuery<Success>
    {
        public Guid VenueId { get; set; }
        public int ProviderUkprn { get; set; }
        public string VenueName { get; set; }
    }
}
