using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Models.Models.Qualifications;
using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface ICourse
    {
        Guid id { get; set; }

        string QualificationCourseTitle { get; set; } // CourseData.CourseTitle
        string LearnAimRef { get; set; } // LARS / QAN: "302309" -??? INTIGER ?
        string NotionalNVQLevelv2 { get; set; } // Level: "7" - ??? INTIGER ?
        string AwardOrgCode { get; set; } // Awarding organisation: "BOLTONIN";
        string QualificationType { get; set; } // ??? QualificationTypes => Diploma, Cerificate or EACH courserun

        string ProviderUKPRN { get; set; } // Or integer 8 digits 

        string CourseDescription { get; set; }
        string EntryRequirments { get; set; } //Requirements { get; }
        string WhatYoullLearn { get; set; }
        string HowYoullLearn { get; set; }
        string WhatYoullNeed { get; set; }
        string HowYoullBeAssessed { get; set; }
        string WhereNext { get; set; }

        bool AdvancedLearnerLoan { get; set; }

        //QuAP QuAP { get; set; }
        //CourseData CourseData { get; set; }
        IEnumerable<CourseRun> CourseRuns { get; }
    }
}