
using System;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Interfaces.Providers;
using Dfc.CourseDirectory.Models.Interfaces.Qualifications;
using Dfc.CourseDirectory.Models.Interfaces.Venues;


namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface ICourseRun
    {
        //Guid id { get; }
        string CourseDescription { get; set; }
        string EntryRequirments { get; set; } //Requirements { get; }
        string WhatYoullLearn { get; set; }
        string HowYoullLearn { get; set; }
        string WhatYoullNeed { get; set; }
        string WhatYoullNeedToBring { get; set; }
        string HowYoullBeAssessed { get; set; }
        string WhereNext { get; set; }
        string CourseName { get; set; }
        string ProviderCourseID { get; set; } //string CourseID { get; }
        string DeliveryMode { get; set; }
        bool FlexibleStartDate { get; set; }
        DateTime StartDate { get; set; }
        string CourseURL { get; set; }
        decimal Cost { get; set; } //string Price { get; }
        string CostDescription { get; set; }
        bool AdvancedLearnerLoan { get; set; }
        DurationUnit DurationUnit { get; set; }
        int DurationValue { get; set; }
        StudyMode StudyMode { get; set; } //string StudyMode { get; }
        AttendancePattern AttendancePattern { get; set; } ////string Attendance { get; }
        //string Pattern { get; }
        IVenue Venue { get; set; }
        IProvider Provider { get; set; }
        IQualification Qualification { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
        DateTime UpdatedDate { get; set; }
        string UpdatedBy { get; set; }
    }
}