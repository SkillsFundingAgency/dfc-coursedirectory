﻿using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class Course
    {
        public Guid CourseId { get; set; }
        public CourseStatus CourseStatus { get; set; }
        public Guid ProviderId { get; set; }
        public string LearnAimRef { get; set; }
        public string CourseDescription { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string WhatYoullNeed { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhereNext { get; set; }
        public IReadOnlyCollection<CourseRun> CourseRuns { get; set; }
    }
}
