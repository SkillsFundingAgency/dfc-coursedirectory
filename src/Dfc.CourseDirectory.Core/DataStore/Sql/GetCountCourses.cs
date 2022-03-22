using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    public class GetCountCourses : ISqlQuery<IReadOnlyCollection<CourseResult>>
        {
            public DateTime Today { get; set; }
        }

    public class CourseResult
    {
        public int TotalCourses { get; set; }
        public int OutofDateCourses { get; set; }
    }
}
