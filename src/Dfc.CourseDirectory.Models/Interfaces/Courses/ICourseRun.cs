
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
        string CourseDescription { get; }
        string EntryRequirments { get; } //Requirements { get; }
        string WhatYoullLearn { get; }
        string HowYoullLearn { get; }
        string WhatYoullNeed { get; }
        string WhatYoullNeedToBring { get; }
        string HowYoullBeAssessed { get; }
        string WhereNext { get; }
        string CourseName { get; }
        string ProviderCourseID { get; } //string CourseID { get; }
        string DeliveryMode { get; }
        bool FlexibleStartDate { get; }
        DateTime StartDate { get; }
        string CourseURL { get; }
        decimal Cost { get; } //string Price { get; }
        string CostDescription { get; }
        bool AdvancedLearnerLoan { get; }
        DurationUnit DurationUnit { get; }
        int DurationValue { get; }
        StudyMode StudyMode { get; } //string StudyMode { get; }
        AttendancePattern AttendancePattern { get; } ////string Attendance { get; }
        //string Pattern { get; }
        IVenue Venue { get; }
        IProvider Provider { get; }
        IQualification Qualification { get; }
        DateTime CreatedDate { get; }
        string CreatedBy { get; }
        DateTime UpdatedDate { get; }
        string UpdatedBy { get; }
    }
}