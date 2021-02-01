using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class DeleteVenue : ICosmosDbQuery<OneOf<NotFound, Success>>
    {
        public Guid VenueId { get; set; }
    }
}
