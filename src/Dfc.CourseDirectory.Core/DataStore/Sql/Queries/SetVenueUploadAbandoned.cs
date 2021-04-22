using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetVenueUploadAbandoned : ISqlQuery<OneOf<NotFound, Success>>
    {
        public Guid VenueUploadId { get; set; }
        public DateTime AbandonedOn { get; set; }
    }
}
