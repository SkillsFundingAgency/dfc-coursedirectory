using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class AuditAndSyncCoursesToIndex : ISqlQuery<int>
    {
        public int MaxCourseCount { get; set; }
        public DateTime Now { get; set; }
    }
}

