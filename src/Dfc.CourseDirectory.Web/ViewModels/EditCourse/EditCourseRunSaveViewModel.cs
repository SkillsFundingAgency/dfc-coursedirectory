using System;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Web.ViewModels.EditCourse
{
    public class EditCourseRunSaveViewModel
    {
        public string CourseName { get; set; }
        public DateTime? StartDate { get; set; }
        public string StartDateType { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string DurationLength { get; set; }
        public string Cost { get; set; }
        public string CostDescription { get; set; }
        public Guid  VenueId { get; set; }
        public bool? National { get; set; }
        public string[] SelectedRegions { get; set; }
        public CourseDeliveryMode DeliveryMode { get; set; }
        public string CourseProviderReference { get; set; }
        public string Url { get; set; }
        public CourseDurationUnit DurationUnit { get; set; }
        public CourseStudyMode? StudyMode { get; set; }
        public CourseAttendancePattern? AttendanceMode { get; set; }
        public Guid? CourseId { get; set; }
        public Guid CourseRunId { get; set; }
        public string QualificationType { get; set; }
        public bool FlexibleStartDate { get; set; }
        public PublishMode Mode { get; set; }
    }
}
