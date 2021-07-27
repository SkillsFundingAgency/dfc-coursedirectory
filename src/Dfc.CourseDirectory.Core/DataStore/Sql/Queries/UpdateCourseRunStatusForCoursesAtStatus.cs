using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpdateCourseRunStatusForCoursesAtStatus : ISqlQuery<Success>
    {
        public Guid ProviderId { get; set; }
        public IEnumerable<CourseStatus> OldStatuses { get; set; }
        public CourseStatus NewStatus { get; set; }
    }
}
