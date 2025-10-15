using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetCourseUploadRowsByCourseId : ISqlQuery<IReadOnlyCollection<CourseUploadRow>>
    {
        public Guid CourseUploadId { get; set; }
        public Guid CourseId { get; set; }
    }
}
