using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql
{
    public class GetExpiredCourseRunsForProvider : ISqlQuery<IReadOnlyCollection<ExpiredCourseRunResult>>
    {
        public Guid ProviderId { get; set; }
        public DateTime Today { get; set; }
    }

    public class ExpiredCourseRunResult
    {
        public Guid CourseId { get; set; }
        public Guid CourseRunId { get; set; }
        public string CourseName { get; set; }
        public string ProviderCourseId { get; set; }
        public string LearnAimRef { get; set; }
        public CourseDeliveryMode DeliveryMode { get; set; }
        public string VenueName { get; set; }
        public bool? National { get; set; }
        public IReadOnlyCollection<string> SubRegionIds { get; set; }
        public CourseStudyMode? StudyMode { get; set; }
        public string LearnAimRefTitle { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string AwardOrgCode { get; set; }
        public string LearnAimRefTypeDesc { get; set; }
        public DateTime StartDate { get; set; }
    }
}
