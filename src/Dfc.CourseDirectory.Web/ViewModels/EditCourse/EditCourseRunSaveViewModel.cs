using System;
using System.ComponentModel;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.Models;

namespace Dfc.CourseDirectory.Web.ViewModels.EditCourse
{
    public class EditCourseRunSaveViewModel
    {
        public string CourseName { get; set; }
        [DisplayName("Start date")]
        public DateTime? StartDate { get; set; }
        public string StartDateType { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string DurationLength { get; set; }
        public int? DurationLengthInt { get => int.TryParse(DurationLength, out var durationLength) ? durationLength : (int?)null; }
        public string Cost { get; set; }
        public decimal? CostDecimal { get => decimal.TryParse(Cost, out var d) ? d : (decimal?)null; }
        public string CostDescription { get; set; }
        public Guid?  VenueId { get; set; }
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
        public bool? FlexibleStartDate { get; set; }
        public PublishMode Mode { get; set; }
        public int? SectorId { get; set; }
        public string SectorDescription { get; set; }
        public EducationLevel? EducationLevel { get; set; }
        public string AwardingBody { get; set; }
        public bool NonLarsCourse { get; set; }
    }
}
