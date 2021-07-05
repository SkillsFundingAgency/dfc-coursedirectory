using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.Queries
{
    public class CreateCourse : ICosmosDbQuery<Success>
    {
        public Guid CourseId { get; set; }
        public Guid ProviderId { get; set; }
        public int ProviderUkprn { get; set; }
        public string QualificationCourseTitle { get; set; }
        public string LearnAimRef { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string AwardOrgCode { get; set; }
        public string QualificationType { get; set; }
        public string CourseDescription { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string WhatYoullNeed { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhereNext { get; set; }
        public bool AdultEducationBudget { get; set; }
        public bool AdvancedLearnerLoan { get; set; }
        public IEnumerable<CreateCourseCourseRun> CourseRuns { get; set; }
        public DateTime CreatedDate { get; set; }
        public UserInfo CreatedByUser { get; set; }
        public CourseStatus CourseStatus { get; set; }
    }

    public class CreateCourseCourseRun
    {
        public Guid CourseRunId { get; set; }
        public Guid? VenueId { get; set; }
        public string CourseName { get; set; }
        public CourseDeliveryMode DeliveryMode { get; set; }
        public bool FlexibleStartDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string CourseUrl { get; set; }
        public decimal? Cost { get; set; }
        public string CostDescription { get; set; }
        public CourseDurationUnit DurationUnit { get; set; }
        public int? DurationValue { get; set; }
        public CourseStudyMode? StudyMode { get; set; }
        public CourseAttendancePattern? AttendancePattern { get; set; }
        public bool? National { get; set; }
        public IEnumerable<string> SubRegionIds { get; set; }
        public string ProviderCourseId { get; set; }
    }
}
