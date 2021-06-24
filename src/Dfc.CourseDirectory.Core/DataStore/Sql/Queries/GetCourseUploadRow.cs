using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetCourseUploadRow : ISqlQuery<CourseUploadRow>
    {
        public Guid CourseUploadId { get; set; }
        public int RowNumber { get; set; }
    }
}
