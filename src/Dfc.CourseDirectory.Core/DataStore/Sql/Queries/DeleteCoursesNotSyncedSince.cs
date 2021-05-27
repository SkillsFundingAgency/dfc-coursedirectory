using System;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class DeleteCoursesNotSyncedSince : ISqlQuery<Success>
    {
        public DateTime SyncedSince { get; set; }
    }
}
