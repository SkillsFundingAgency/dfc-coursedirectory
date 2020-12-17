using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Search.Models;

namespace Dfc.CourseDirectory.FindACourseApi.Features.CourseRunDetail
{
    public class CourseRunDetailViewModel
    {
        public FindACourseOfferingType OfferingType { get; set; }
        public ProviderViewModel Provider { get; set; }
        public CourseViewModel Course { get; set; }
        public VenueViewModel Venue { get; set; }
        public QualificationViewModel Qualification { get; set; }
        public IEnumerable<AlternativeCourseRunViewModel> AlternativeCourseRuns { get; set; }
        public Guid CourseRunId { get; set; }
        public CourseAttendancePattern? AttendancePattern { get; set; }
        public decimal? Cost { get; set; }
        public string CostDescription { get; set; }
        public string CourseName { get; set; }
        public string CourseURL { get; set; }
        public DateTime CreatedDate { get; set; }
        public CourseDeliveryMode DeliveryMode { get; set; }
        public CourseDurationUnit DurationUnit { get; set; }
        public int? DurationValue { get; set; }
        public bool? National { get; set; }
        public bool FlexibleStartDate { get; set; }
        public DateTime? StartDate { get; set; }
        public CourseStudyMode StudyMode { get; set; }
        public IEnumerable<SubRegionViewModel> SubRegions { get; set; }
    }
}
