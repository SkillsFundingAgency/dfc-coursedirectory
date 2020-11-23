using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertCoursesFromCosmos : ISqlQuery<None>
    {
        public IEnumerable<UpsertCoursesRecord> Records { get; set; }
        public DateTime LastSyncedFromCosmos { get; set; }
    }

    public class UpsertCoursesRecord
    {
        public Guid CourseId { get; set; }
        public CourseStatus CourseStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public int? TribalCourseId { get; set; }
        public string LearnAimRef { get; set; }
        public int ProviderUkprn { get; set; }
        public string CourseDescription { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string WhatYoullNeed { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhereNext { get; set; }
        public IEnumerable<UpsertCoursesRecordCourseRun> CourseRuns { get; set; }
    }

    public class UpsertCoursesRecordCourseRun
    {
        public Guid CourseRunId { get; set; }
        public CourseStatus CourseRunStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public string CourseName { get; set; }
        public Guid? VenueId { get; set; }
        public string ProviderCourseId { get; set; }
        public CourseDeliveryMode DeliveryMode { get; set; }
        public bool FlexibleStartDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string CourseWebsite { get; set; }
        public decimal? Cost { get; set; }
        public string CostDescription { get; set; }
        public CourseDurationUnit? DurationUnit { get; set; }
        public int? DurationValue { get; set; }
        public CourseStudyMode? StudyMode { get; set; }
        public CourseAttendancePattern? AttendancePattern { get; set; }
        public bool? National { get; set; }
        public IEnumerable<string> Regions { get; set; }
    }
}
