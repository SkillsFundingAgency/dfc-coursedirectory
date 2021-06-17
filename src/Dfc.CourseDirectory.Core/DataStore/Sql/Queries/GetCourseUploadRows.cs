using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetCourseUploadRows : ISqlQuery<(IReadOnlyCollection<CourseUploadRow> Rows, int LastRowNumber)>
    {
        public Guid CourseUploadId { get; set; }
    }
}
