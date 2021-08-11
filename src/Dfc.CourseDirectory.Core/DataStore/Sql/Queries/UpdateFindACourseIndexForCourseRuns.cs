using System;
using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpdateFindACourseIndexForCourseRuns : ISqlQuery<Success>
    {
        public IEnumerable<Guid> CourseRunIds { get; set; }
        public DateTime Now { get; set; }
    }
}
