using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class PublishVenueUpload : ISqlQuery<OneOf<NotFound, PublishVenueUploadResult>>
    {
        public Guid VenueUploadId { get; set; }
        public UserInfo PublishedBy { get; set; }
        public DateTime PublishedOn { get; set; }
    }

    public class PublishVenueUploadResult
    {
        public int PublishedCount { get; set; }
    }
}
