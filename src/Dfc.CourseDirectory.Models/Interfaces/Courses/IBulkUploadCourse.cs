using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface IBulkUploadCourse
    {
        bool IsCourseHeader { get; set; }
        int BulkUploadLineNumber { get; set; }
        int TempCourseId { get; set; }

        // Course
        string QualificationCourseTitle { get; set; }
        string LearnAimRef { get; set; }
        string NotionalNVQLevelv2 { get; set; }
        string AwardOrgCode { get; set; }
        string QualificationType { get; set; }

        int ProviderUKPRN { get; set; }

        string CourseDescription { get; set; }
        string EntryRequirements { get; set; }
        string WhatYoullLearn { get; set; }
        string HowYoullLearn { get; set; }
        string WhatYoullNeed { get; set; }
        string HowYoullBeAssessed { get; set; }
        string WhereNext { get; set; }
        string AdultEducationBudget { get; set; } // bool
        string AdvancedLearnerLoan { get; set; } // bool

        // CourseRun
        string VenueName { get; set; } // => Guid? VenueId
        string CourseName { get; set; }
        string ProviderCourseID { get; set; }
        string DeliveryMode { get; set; } // DeliveryMode DeliveryMode
        string FlexibleStartDate { get; set; } // bool - Yes/No    
        string StartDate { get; set; } // ???? DateTime?
        string CourseURL { get; set; }
        string Cost { get; set; } // ??? decimal?
        string CostDescription { get; set; }
        string DurationUnit { get; set; } // DurationUnit
        string DurationValue { get; set; } // ??? int?
        string StudyMode { get; set; } // StudyMode
        string AttendancePattern { get; set; } // AttendancePattern
    }
}
