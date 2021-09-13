using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class PublishApprenticeshipUpload : ISqlQuery<OneOf<NotFound, PublishApprenticeshipUploadResult>>
    {
        public Guid ApprenticeshipUploadId { get; set; }
        public UserInfo PublishedBy { get; set; }
        public DateTime PublishedOn { get; set; }
    }

    public class PublishApprenticeshipUploadResult
    {
        public int PublishedCount { get; set; }
    }
}
