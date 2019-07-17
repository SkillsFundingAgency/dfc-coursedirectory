using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface ICsvCourse
    {
        string LearnAimRef { get; set; }
        string CourseDescription { get; set; }
        string EntryRequirements { get; set; }
        string WhatYoullLearn { get; set; }
        string HowYoullLearn { get; set; }
        string WhatYoullNeed { get; set; }
        string HowYoullBeAssessed { get; set; }
        string WhereNext { get; set; }
        string AdvancedLearnerLoan { get; set; }
        string AdultEducationBudget { get; set; }
        string CourseName { get; set; }
        string ProviderCourseID { get; set; }
        string DeliveryMode { get; set; }
        string StartDate { get; set; }
        string FlexibleStartDate { get; set; }
        string VenueName { get; set; }
        string National { get; set; }
        string Regions { get; set; }
        string SubRegions { get; set; }
        string CourseURL { get; set; }
        string Cost { get; set; }
        string CostDescription { get; set; }
        string DurationValue { get; set; }
        string DurationUnit { get; set; }
        string StudyMode { get; set; }
        string AttendancePattern { get; set; }
    }
}
