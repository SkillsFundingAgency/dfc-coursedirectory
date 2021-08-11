using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class PublishCourseUpload : ISqlQuery<OneOf<NotFound, PublishCourseUploadResult>>
    {
        public Guid CourseUploadId { get; set; }
        public UserInfo PublishedBy { get; set; }
        public DateTime PublishedOn { get; set; }
    }

    public class PublishCourseUploadResult
    {
        public IReadOnlyCollection<Guid> PublishedCourseRunIds { get; set; }
    }
}
