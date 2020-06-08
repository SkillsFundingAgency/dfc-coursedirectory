using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models
{
    public class Course
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        public Guid ProviderId { get; set; }
        public int ProviderUKPRN { get; set; }
        public int? CourseId { get; set; }
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
        public IEnumerable<CourseRun> CourseRuns { get; set; }
        public IEnumerable<string> ValidationErrors { get; set; }
        public IEnumerable<BulkUploadError> BulkUploadErrors { get; set; }
        public int CourseStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class CourseRun
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        public int? CourseInstanceId { get; set; }
        public Guid? VenueId { get; set; }
        public string CourseName { get; set; }
        public string ProviderCourseID { get; set; }
        public int DeliveryMode { get; set; }
        public bool FlexibleStartDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string CourseURL { get; set; }
        public decimal? Cost { get; set; }
        public string CostDescription { get; set; }
        public int DurationUnit { get; set; }
        public int? DurationValue { get; set; }
        public int StudyMode { get; set; }
        public int AttendancePattern { get; set; }
        public bool? National { get; set; }
        public IEnumerable<string> Regions { get; set; }
        public int RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        //public IEnumerable<SubRegionItemModel> SubRegions { get; set; }
        public IEnumerable<string> ValidationErrors { get; set; }
        public IEnumerable<BulkUploadError> BulkUploadErrors { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}
