using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpdateVenueUploadStatus : ISqlQuery<OneOf<NotFound, Success>>
    {
        public Guid VenueUploadId { get; set; }
        public UploadStatus UploadStatus { get; set; }
        public DateTime ChangedOn { get; set; }
    }
}
