using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class DeleteCourseUploadRow : ISqlQuery<OneOf<Success, NotFound>>
    {
        public Guid CourseUploadId { get; set; }
        public int RowNumber { get; set; }
    }
}
