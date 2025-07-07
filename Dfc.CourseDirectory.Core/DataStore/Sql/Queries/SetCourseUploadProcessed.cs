using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetCourseUploadProcessed : ISqlQuery<OneOf<NotFound, Success>>
    {
        public Guid CourseUploadId { get; set; }
        public DateTime ProcessingCompletedOn { get; set; }
        public bool IsValid { get; set; }
    }
}
