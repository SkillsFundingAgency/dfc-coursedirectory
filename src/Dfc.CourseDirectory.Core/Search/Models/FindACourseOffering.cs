using System;
using System.Text.Json.Serialization;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.Spatial;

namespace Dfc.CourseDirectory.Core.Search.Models
{
    public enum FindACourseOfferingType
    {
        Course = 1,
        TLevel = 2
    }

    public class FindACourseOffering
    {
        public string id { get; set; }
        public FindACourseOfferingType OfferingType { get; set; } = FindACourseOfferingType.Course;
        public Guid? CourseId { get; set; }
        public Guid? CourseRunId { get; set; }
        public Guid? TLevelId { get; set; }
        public string QualificationCourseTitle { get; set; }
        public string LearnAimRef { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string VenueName { get; set; }
        public string VenueAddress { get; set; }
        [JsonConverter(typeof(Azure.Core.Serialization.MicrosoftSpatialGeoJsonConverter))]
        public GeographyPoint VenueLocation { get; set; }
        public string VenueAttendancePattern { get; set; }
        public string VenueAttendancePatternDescription { get; set; }
        public string ProviderName { get; set; }
        public string Region { get; set; }
        public decimal ScoreBoost { get; set; }
        public int? Status { get; set; }
        public string VenueStudyMode { get; set; }
        public string VenueStudyModeDescription { get; set; }
        public string DeliveryMode { get; set; }
        public string DeliveryModeDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public string VenueTown { get; set; }
        public int? Cost { get; set; }
        public string CostDescription { get; set; }
        public string CourseText { get; set; }
        public string UKPRN { get; set; }
        public string CourseDescription { get; set; }
        public string CourseName { get; set; }
        public bool? FlexibleStartDate { get; set; }
        public CourseDurationUnit? DurationUnit { get; set; }
        public int? DurationValue { get; set; }
        public bool? National { get; set; }
    }
}
