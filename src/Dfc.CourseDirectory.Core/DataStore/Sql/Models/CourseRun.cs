using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class CourseRun
    {
        public Guid CourseRunId { get; set; }
        public CourseStatus CourseRunStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string CourseName { get; set; }
        public Guid? VenueId { get; set; }
        public string ProviderCourseId { get; set; }
        public CourseDeliveryMode DeliveryMode { get; set; }
        public bool FlexibleStartDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string CourseWebsite { get; set; }
        public decimal? Cost { get; set; }
        public string CostDescription { get; set; }
        public CourseDurationUnit? DurationUnit { get; set; }
        public int? DurationValue { get; set; }
        public CourseStudyMode? StudyMode { get; set; }
        public CourseAttendancePattern? AttendancePattern { get; set; }
        public bool? National { get; set; }
        public IReadOnlyCollection<string> SubRegionIds { get; set; }
        public string VenueName { get; set; }
        public string ProviderVenueRef { get; set; }
    }
}
