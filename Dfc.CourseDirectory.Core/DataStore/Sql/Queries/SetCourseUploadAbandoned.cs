using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetCourseUploadAbandoned : ISqlQuery<OneOf<NotFound, Success>>
    {
        public Guid CourseUploadId { get; set; }
        public DateTime AbandonedOn { get; set; }
    }
}
