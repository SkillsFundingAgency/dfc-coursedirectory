using System;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class ArchiveProviderCourses : ISqlQuery<Success>
    {
        public DateTime RetentionDate { get; set; }
    }
}
