using System;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Search.Models;

namespace Dfc.CourseDirectory.FindACourseApi.Features.Search
{
    public class SearchResultViewModel
    {
        public double SearchScore { get; set; }
        public double? Distance { get; set; }
        public CoordinatesViewModel VenueLocation { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? CourseRunId { get; set; }
        public FindACourseOfferingType OfferingType { get; set; }
        public Guid? TLevelId { get; set; }
        public Guid? TLevelLocationId { get; set; }
        public string QualificationCourseTitle { get; set; }
        public string LearnAimRef { get; set; }
        public string QualificationLevel { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string VenueName { get; set; }
        public string VenueAddress { get; set; }
        public string VenueAttendancePattern { get; set; }
        public string VenueAttendancePatternDescription { get; set; }
        public string ProviderName { get; set; }
        public string Region { get; set; }
        public string VenueStudyMode { get; set; }
        public string VenueStudyModeDescription { get; set; }
        public string DeliveryMode { get; set; }
        public string DeliveryModeDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public string VenueTown { get; set; }
        public int? Cost { get; set; }
        public string CostDescription { get; set; }
        public string CourseText { get; set; }
        public string Ukprn { get; set; }
        public string CourseDescription { get; set; }
        public string CourseName { get; set; }
        public bool? FlexibleStartDate { get; set; }
        public CourseDurationUnit DurationUnit { get; set; }
        public int? DurationValue { get; set; }
        public bool? National { get; set; }
    }
}
