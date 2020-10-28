using System;
using System.Collections.Generic;
using Dfc.ProviderPortal.FindACourse.Models;

namespace Dfc.ProviderPortal.FindACourse.ApiModels
{
    public class CourseRunDetailResponse
    {
        public CourseDetailResponseProvider Provider { get; set; }
        public CourseDetailResponseCourse Course { get; set; }
        public CourseDetailResponseVenue Venue { get; set; }
        public CourseDetailResponseQualification Qualification { get; set; }
        public IEnumerable<CourseDetailResponseAlternativeCourseRun> AlternativeCourseRuns { get; set; }
        public Guid CourseRunId { get; set; }
        public AttendancePattern? AttendancePattern { get; set; }
        public decimal? Cost { get; set; }
        public string CostDescription { get; set; }
        public string CourseName { get; set; }
        public string CourseURL { get; set; }
        public DateTime CreatedDate { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public DurationUnit DurationUnit { get; set; }
        public int? DurationValue { get; set; }
        public bool? National { get; set; }
        public bool FlexibleStartDate { get; set; }
        public DateTime? StartDate { get; set; }
        public StudyMode StudyMode { get; set; }
        public IEnumerable<CourseDetailResponseSubRegion> SubRegions { get; set; }
    }

    public class CourseDetailResponseProvider
    {
        public string ProviderName { get; set; }
        public string TradingName { get; set; }
        public string CourseDirectoryName { get; set; }
        public string Alias { get; set; }
        public string UKPRN { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string Postcode { get; set; }
        public string County { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Fax { get; set; }
        public string Website { get; set; }
        public decimal? LearnerSatisfaction { get; set; }
        public decimal? EmployerSatisfaction { get; set; }
    }

    public class CourseDetailResponseCourse
    {
        public bool AdvancedLearnerLoan { get; set; }
        public string AwardOrgCode { get; set; }
        public string CourseDescription { get; set; }
        public Guid CourseId { get; set; }
        public string LearnAimRef { get; set; }
        public string QualificationLevel { get; set; }
        public string WhatYoullLearn { get; set; }
        public string WhatYoullNeed { get; set; }
        public string WhereNext { get; set; }
        public string EntryRequirements { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string HowYoullLearn { get; set; }
    }

    public class CourseDetailResponseVenue
    {
        public string VenueName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }

    public class CourseDetailResponseQualification
    {
        public string LearnAimRef { get; set; }
        public string LearnAimRefTitle { get; set; }
        public string LearnAimRefTypeDesc { get; set; }
        public string QualificationLevel { get; set; }
        public string AwardOrgCode { get; set; }
        public string AwardOrgName { get; set; }
        public string SectorSubjectAreaTier1Desc { get; set; }
        public string SectorSubjectAreaTier2Desc { get; set; }
    }

    public class CourseDetailResponseAlternativeCourseRun
    {
        public Guid CourseRunId { get; set; }
        public AttendancePattern? AttendancePattern { get; set; }
        public decimal? Cost { get; set; }
        public string CostDescription { get; set; }
        public string CourseName { get; set; }
        public string CourseURL { get; set; }
        public DateTime CreatedDate { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public DurationUnit DurationUnit { get; set; }
        public int? DurationValue { get; set; }
        public bool FlexibleStartDate { get; set; }
        public DateTime? StartDate { get; set; }
        public StudyMode StudyMode { get; set; }
        public CourseDetailResponseVenue Venue { get; set; }
    }

    public class CourseDetailResponseRegion
    {
        public string RegionId { get; set; }
        public string Name { get; set; }
    }

    public class CourseDetailResponseSubRegion
    {
        public string SubRegionId { get; set; }
        public string Name { get; set; }
        public CourseDetailResponseRegion ParentRegion { get; set; }
    }
}
