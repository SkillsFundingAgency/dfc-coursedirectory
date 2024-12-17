using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class AuditAndSyncCourseRunsToIndex : ISqlQuery<int>
    {
        public int MaxCourseRunCount { get; set; }
        public DateTime Now { get; set; }
    }
}

