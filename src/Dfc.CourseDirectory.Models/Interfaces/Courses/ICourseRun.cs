
using System;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Interfaces.Providers;
using Dfc.CourseDirectory.Models.Interfaces.Qualifications;
using Dfc.CourseDirectory.Models.Interfaces.Venues;


namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface ICourseRun
    {
        Guid id { get; set; }
        Guid VenueId { get; set; }
       
        string CourseName { get; set; }
        string ProviderCourseID { get; set; }
        DeliveryMode DeliveryMode { get; set; }
        bool FlexibleStartDate { get; set; }
        DateTime StartDate { get; set; }
        string CourseURL { get; set; }
        decimal Cost { get; set; } 
        string CostDescription { get; set; }
        DurationUnit DurationUnit { get; set; }
        int DurationValue { get; set; }
        StudyMode StudyMode { get; set; } 
        AttendancePattern AttendancePattern { get; set; } 

        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
        DateTime UpdatedDate { get; set; }
        string UpdatedBy { get; set; }
    }
}