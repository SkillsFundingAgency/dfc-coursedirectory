using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Models.Models.Qualifications;
using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface ICourseText
    {
         Guid ID { get; set; }
         string QualificationCourseTitle { get; set; }
         string LearnAimRef { get; set; }
         string NotionalNVQLevelv2 { get; set; }
         string TypeName { get; set; }
         string AwardOrgCode { get; set; }
         string CountOfOpportunities { get; set; }
         string CourseDescription { get; set; }
         string EntryRequirements { get; set; }
         string WhatYoullLearn { get; set; }
         string HowYoullLearn { get; set; }
         string WhatYoullNeed { get; set; }
         string HowYoullBeAssessed { get; set; }
         string WhereNext
        {
            get; set;
        }
    }
}