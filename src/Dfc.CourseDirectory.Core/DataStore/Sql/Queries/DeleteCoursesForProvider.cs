using System;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class DeleteCoursesForProvider : ISqlQuery<Success>
    {
        public Guid ProviderId { get; set; }
    }
}
