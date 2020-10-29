
using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Spatial;
using Dfc.CourseDirectory.FindACourseApi.Models;


namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface IAzureSearchCourse
    {
        string id { get; set; }
        Guid CourseId { get; set; }
        Guid CourseRunId { get; set; }
        string QualificationCourseTitle { get; set; }
        string LearnAimRef { get; set; }
        string NotionalNVQLevelv2 { get; set; }
        DateTime? UpdatedOn { get; set; }
        string VenueName { get; set; }
        string VenueAddress { get; set; }
        GeographyPoint VenueLocation { get; set; }
        string VenueAttendancePattern { get; set; }
        string VenueAttendancePatternDescription { get; set; }
        string ProviderName { get; set; }
        string Region { get; set; }
        decimal ScoreBoost { get; set; }
        int? Status { get; set; }
        string VenueStudyMode { get; set; }
        string VenueStudyModeDescription { get; set; }
        string DeliveryMode { get; set; }
        string DeliveryModeDescription { get; set; }
        DateTime? StartDate { get; set; }
        string VenueTown { get; set; }
        int? Cost { get; set; }
        string CostDescription { get; set; }
        string CourseText { get; set; }
        string UKPRN { get; set; }
        string CourseDescription { get; set; }
    }
}
