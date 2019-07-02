using Dfc.CourseDirectory.Models.Interfaces.Courses;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class BulkUploadCourse : IBulkUploadCourse
    {
        public bool IsCourseHeader { get; set; }
        public int BulkUploadLineNumber { get; set; }
        public int TempCourseId { get; set; }

        // Course
        public string QualificationCourseTitle { get; set; }
        public string LearnAimRef { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string AwardOrgCode { get; set; }
        public string QualificationType { get; set; }

        public int ProviderUKPRN { get; set; }

        public string CourseDescription { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string WhatYoullNeed { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhereNext { get; set; }
        public string AdultEducationBudget { get; set; } // bool
        public string AdvancedLearnerLoan { get; set; } // bool

        // CourseRun
        public string VenueName { get; set; } // => Guid? VenueId
        public string CourseName { get; set; }
        public string ProviderCourseID { get; set; }
        public string DeliveryMode { get; set; } // DeliveryMode DeliveryMode
        public string FlexibleStartDate { get; set; } // bool - Yes/No    
        public string StartDate { get; set; } // ???? DateTime?
        public string CourseURL { get; set; }
        public string Cost { get; set; } // ??? decimal?
        public string CostDescription { get; set; }
        public string DurationUnit { get; set; } // DurationUnit
        public string DurationValue { get; set; } // ???
        public string StudyMode { get; set; } // StudyMode
        public string AttendancePattern { get; set; } // AttendancePattern
        public string National { get; set; }
        public string Regions { get; set; }
        public string SubRegions { get; set; }
    }
}
