using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetCourseRunCountsByStatusForProvider : ISqlQuery<IReadOnlyDictionary<CourseStatus, int>>
    {
        public Guid ProviderId { get; set; }
    }
}
