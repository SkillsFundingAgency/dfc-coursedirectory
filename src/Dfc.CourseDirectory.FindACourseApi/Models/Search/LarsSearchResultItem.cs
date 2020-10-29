
using System;


namespace Dfc.CourseDirectory.FindACourseApi.Models
{
    public class LARSSearchResultItem
    {
        public decimal SearchScore { get; }
        public string LearnAimRef { get; }
        public string LearnAimRefTitle { get; }
        public string NotionalNVQLevelv2 { get; }
        public string AwardOrgCode { get; }
        public string LearnDirectClassSystemCode1 { get; }
        public string LearnDirectClassSystemCode2 { get; }
        public string GuidedLearningHours { get; }
        public string TotalQualificationTime { get; }
        public string UnitType { get; }
        public string AwardOrgName { get; }
        public string LearnAimRefTypeDesc { get; }
        public DateTime? CertificationEndDate { get; }
        public string SectorSubjectAreaTier1Desc { get; }
        public string SectorSubjectAreaTier2Desc { get; }
        public string AwardOrgAimRef { get; }
    }
}
