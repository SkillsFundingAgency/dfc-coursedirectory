using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class DeleteVenue : ICosmosDbQuery<OneOf<NotFound, Success>>
    {
        public Guid VenueId { get; set; }
        public DateTime UpdatedDate { get; set; }
        public UserInfo UpdatedBy { get; set; }
    }
}
