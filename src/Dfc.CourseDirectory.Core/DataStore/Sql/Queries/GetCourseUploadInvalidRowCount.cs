using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetCourseUploadInvalidRowCount : ISqlQuery<int>
    {
        public Guid CourseUploadId { get; set; }
    }
}
