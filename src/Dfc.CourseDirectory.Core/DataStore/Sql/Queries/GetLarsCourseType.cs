﻿using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetLarsCourseType : ISqlQuery<IReadOnlyCollection<LarsCourseType>>
    {
        public string LearnAimRef { get; set; }
    }
}
