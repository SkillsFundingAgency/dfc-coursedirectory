using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Models.Models.Qualifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class Course : ICourse 
    {
        public Guid id { get; set; }

        public string QualificationCourseTitle { get; set; } 
        public string LearnAimRef { get; set; } 
        public string NotionalNVQLevelv2 { get; set; } 
        public string AwardOrgCode { get; set; } // Awarding organisation: "BOLTONIN";
        public string QualificationType { get; set; } // ??? QualificationTypes => Diploma, Cerificate or EACH courserun

        public string ProviderUKPRN { get; set; } // Or integer 8 digits 


        public string CourseDescription { get; set; }
        public string EntryRequirments { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string WhatYoullNeed { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhereNext { get; set; }

        public bool AdvancedLearnerLoan { get; set; }

        //public QuAP QuAP { get; set; }
        //public CourseData CourseData { get; set; }
        public IEnumerable<CourseRun> CourseRuns { get; set; }

        //public Course(
        //    Guid id,
        //    QuAP quAP,
        //    CourseData data,
        //    IEnumerable<CourseRun> run)
        //{
        //    Throw.IfNull(id, nameof(id));
        //    Throw.IfNull(quAP, nameof(quAP));
        //    Throw.IfNull(data, nameof(data));
        //    Throw.IfNull(data, nameof(run));

        //    ID = id;
        //    QuAP = quAP;
        //    CourseData = data;
        //}

        //protected override IEnumerable<object> GetEqualityComponents()
        //{
        //    yield return ID;
        //    yield return QuAP;
        //    yield return CourseData;
        //}
    }
}