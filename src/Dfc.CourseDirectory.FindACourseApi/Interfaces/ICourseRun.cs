
using Dfc.CourseDirectory.FindACourseApi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dfc.CourseDirectory.FindACourseApi.Models;


namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface ICourseRun
    {
        Guid id { get; set; }
        Guid? VenueId { get; set; }

        string CourseName { get; set; }
        string ProviderCourseID { get; set; }
        DeliveryMode DeliveryMode { get; set; }
        bool FlexibleStartDate { get; set; }
        DateTime? StartDate { get; set; }
        string CourseURL { get; set; }
        decimal? Cost { get; set; }
        string CostDescription { get; set; }
        DurationUnit DurationUnit { get; set; }
        int? DurationValue { get; set; }
        StudyMode StudyMode { get; set; }
        AttendancePattern AttendancePattern { get; set; }
        IEnumerable<string> Regions { get; set; }
        RecordStatus RecordStatus { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedDate { get; set; }
        string UpdatedBy { get; set; }
    }
}