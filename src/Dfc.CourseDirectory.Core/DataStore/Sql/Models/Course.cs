using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class Course
    {
        public Guid CourseId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid ProviderId { get; set; }
        public int ProviderUkprn { get; set; }
        public string LearnAimRef { get; set; }
        public string CourseDescription { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string WhatYoullNeed { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhereNext { get; set; }
        public IReadOnlyCollection<CourseRun> CourseRuns { get; set; }
        public string LearnAimRefTypeDesc { get; set; }
        public string AwardOrgCode { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string LearnAimRefTitle { get; set; }
    }
}
