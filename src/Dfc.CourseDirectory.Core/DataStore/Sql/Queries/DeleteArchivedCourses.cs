using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class DeleteArchivedCourses : ISqlQuery<int>
    {
        public DateTime RetentionDate { get; set; }
    }
}
