using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Models.Models.Qualifications;
using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface ICourse
    {
        Guid Id { get; set; }

        string QualificationCourseTitle { get; set; } 
        string LearnAimRef { get; set; } 
        string NotionalNVQLevelv2 { get; set; } 
        string AwardOrgCode { get; set; } 
        string QualificationType { get; set; } 

        string ProviderUKPRN { get; set; } 

        string CourseDescription { get; set; }
        string EntryRequirments { get; set; } 
        string WhatYoullLearn { get; set; }
        string HowYoullLearn { get; set; }
        string WhatYoullNeed { get; set; }
        string HowYoullBeAssessed { get; set; }
        string WhereNext { get; set; }

        bool AdvancedLearnerLoan { get; set; }

        IEnumerable<CourseRun> CourseRuns { get; }
    }
}