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
        public string Id { get; set; }
        public FindACourseOfferingType OfferingType { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? CourseRunId { get; set; }
        public Guid? TLevelId { get; set; }
        public bool Live { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid ProviderId { get; set; }
        public string ProviderDisplayName { get; set; }
        public int ProviderUkprn { get; set; }
        public string QualificationCourseTitle { get; set; }
        public string LearnAimRef { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string CourseDescription { get; set; }
        public string CourseName { get; set; }
        public CourseDeliveryMode DeliveryMode { get; set; }
        public bool? FlexibleStartDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string Cost { get; set; }
        public string CostDescription { get; set; }
        public CourseDurationUnit? DurationUnit { get; set; }
        public int? DurationValue { get; set; }
        public CourseStudyMode StudyMode { get; set; }
        public CourseAttendancePattern? AttendancePattern { get; set; }
        public bool? National { get; set; }
        public string VenueName { get; set; }
        public string VenueAddress { get; set; }
        public string VenueTown { get; set; }
        [JsonConverter(typeof(Azure.Core.Serialization.MicrosoftSpatialGeoJsonConverter))]
        public GeographyPoint Position { get; set; }
        public string RegionName { get; set; }
        public decimal ScoreBoost { get; set; }
    }
}
