using System;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class DeleteArchivedCourses : ISqlQuery<Success>
    {
        public DateTime RetentionDate { get; set; }
    }
}
