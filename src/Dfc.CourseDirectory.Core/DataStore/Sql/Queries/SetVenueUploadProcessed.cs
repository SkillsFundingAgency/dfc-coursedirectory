using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetVenueUploadProcessed : ISqlQuery<OneOf<NotFound, Success>>
    {
        public Guid VenueUploadId { get; set; }
        public DateTime ProcessingCompletedOn { get; set; }
        public bool IsValid { get; set; }
    }
}
