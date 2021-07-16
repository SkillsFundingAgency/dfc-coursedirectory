using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class CourseUploadRowUpdate
    {
        public CourseDeliveryMode DeliveryMode { get; set; }
        public string CourseName { get; set; }
        public string ProviderCourseRef { get; set; }
        public DateTime? StartDate { get; set; }
        public bool FlexibleStartDate { get; set; }
        public bool? NationalDelivery { get; set; }
        public IReadOnlyCollection<string> SubRegionIds { get; set; }
        public string CourseWebPage { get; set; }
        public decimal? Cost { get; set; }
        public string CostDescription { get; set; }
        public int Duration { get; set; }
        public CourseDurationUnit DurationUnit { get; set; }
        public CourseStudyMode? StudyMode { get; set; }
        public CourseAttendancePattern? AttendancePattern { get; set; }
        public Guid? VenueId { get; set; }
    }
}
