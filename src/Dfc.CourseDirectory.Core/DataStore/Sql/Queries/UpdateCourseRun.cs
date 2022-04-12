using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpdateCourseRun : ISqlQuery<OneOf<NotFound, Success>>
    {
        
        public Guid [] SelectedCourseRunid { get; set; }

        public Guid CourseRunId { get; set; }
        public string CourseName { get; set; }
        public CourseDeliveryMode DeliveryMode { get; set; }
        public bool FlexibleStartDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string CourseUrl { get; set; }
        public decimal? Cost { get; set; }
        public string CostDescription { get; set; }
        public CourseDurationUnit DurationUnit { get; set; }
        public int DurationValue { get; set; }
        public string ProviderCourseId { get; set; }
        public bool? National { get; set; }
        public IEnumerable<string> SubRegionIds { get; set; }
        public Guid? VenueId { get; set; }
        public CourseAttendancePattern? AttendancePattern { get; set; }
        public CourseStudyMode? StudyMode { get; set; }
        public UserInfo UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
