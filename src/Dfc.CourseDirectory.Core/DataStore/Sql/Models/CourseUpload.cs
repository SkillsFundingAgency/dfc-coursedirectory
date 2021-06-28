using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class CourseUpload
    {
        public Guid CourseUploadId { get; set; }
        public Guid ProviderId { get; set; }
        public UploadStatus UploadStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ProcessingStartedOn { get; set; }
        public DateTime? ProcessingCompletedOn { get; set; }
        public DateTime? PublishedOn { get; set; }
        public DateTime? AbandonedOn { get; set; }
        public DateTime? LastValidated { get; set; }
    }

    public class CourseUploadRow
    {
        public int RowNumber { get; set; }
        public bool IsValid { get; set; }
        public IReadOnlyCollection<string> Errors { get; set; }
        public Guid CourseId { get; set; }
        public Guid CourseRunId { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime LastValidated { get; set; }
        public string LarsQan { get; set; }
        public string WhoThisCourseIsFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYouWillLearn { get; set; }
        public string HowYouWillLearn { get; set; }
        public string WhatYouWillNeedToBring { get; set; }
        public string HowYouWillBeAssessed { get; set; }
        public string WhereNext { get; set; }
        public string CourseName { get; set; }
        public string ProviderCourseRef { get; set; }
        public string DeliveryMode { get; set; }
        public string StartDate { get; set; }
        public string FlexibleStartDate { get; set; }
        public string VenueName { get; set; }
        public string ProviderVenueRef { get; set; }
        public string NationalDelivery { get; set; }
        public string SubRegions { get; set; }
        public string CourseWebpage { get; set; }
        public string Cost { get; set; }
        public string CostDescription { get; set; }
        public string Duration { get; set; }
        public string DurationUnit { get; set; }
        public string StudyMode { get; set; }
        public string AttendancePattern { get; set; }
        public Guid? VenueId { get; set; }
    }
}
