using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Models.Regions;

namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface ICourseRun
    {
        Guid id { get; set; }
        int? CourseInstanceId { get; set; }
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
        bool? National { get; set; }
        IEnumerable<string> Regions { get; }
        RecordStatus RecordStatus { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedDate { get; set; }
        string UpdatedBy { get; set; }
        IEnumerable<SubRegionItemModel> SubRegions { get; set; }
        IEnumerable<string> ValidationErrors { get; set; }
        IEnumerable<BulkUploadError> BulkUploadErrors { get; set; }
    }
}