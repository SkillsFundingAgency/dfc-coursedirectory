using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class DeleteCourseUploadRowGroup : ISqlQuery<OneOf<NotFound, Success>>
    {
        public Guid CourseUploadId { get; set; }
        public Guid CourseId { get; set; }
    }
}
