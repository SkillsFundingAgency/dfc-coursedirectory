using System;
using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpdateFindACourseIndexForExpiredFreeCourseFunding : ISqlQuery<Success>
    {
        public string LearnAimRef { get; set; }
    }
}
