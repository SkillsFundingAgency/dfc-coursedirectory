﻿using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpdateFindACourseIndexFromMissingCourses : ISqlQuery<int>
    {
        public int MaxCourseRunCount { get; set; }
        public DateTime CreatedAfter { get; set; }
        public DateTime CreatedBefore { get; set; }
        public DateTime Now { get; set; }
    }
}
