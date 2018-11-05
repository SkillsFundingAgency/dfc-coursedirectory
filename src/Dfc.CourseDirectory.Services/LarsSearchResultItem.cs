using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces;

namespace Dfc.CourseDirectory.Services
{
    public class LarsSearchResultItem : ILarsSearchResultItem
    {
        public decimal SearchScore { get; }
        public string LearnAimRef { get; }
        public string LearnAimRefTitle { get; }
        public string NotionalNVQLevelv2 { get; }
        public string AwardOrgCode { get; }
        public string LearnDirectClassSystemCode1 { get; }
        public string LearnDirectClassSystemCode2 { get; }
        public string SectorSubjectAreaTier1 { get; }
        public string SectorSubjectAreaTier2 { get; }
        public string GuidedLearningHours { get; }
        public string TotalQualificationTime { get; }
        public string UnitType { get; }
        public string AwardOrgName { get; }

        public LarsSearchResultItem(
            decimal searchScore,
            string learnAimRef,
            string notionalNVQLevelv2,
            string awardOrgCode,
            string learnDirectClassSystemCode1,
            string learnDirectClassSystemCode2,
            string guidedLearningHours,
            string totalQualificationTime,
            string unitType,
            string awardOrgName)
        {
            Throw.IfLessThan(0, searchScore, nameof(searchScore));
            Throw.IfNullOrWhiteSpace(learnAimRef, nameof(learnAimRef));
            Throw.IfNullOrWhiteSpace(notionalNVQLevelv2, nameof(notionalNVQLevelv2));
            Throw.IfNullOrWhiteSpace(notionalNVQLevelv2, nameof(notionalNVQLevelv2));
            Throw.IfNullOrWhiteSpace(awardOrgCode, nameof(awardOrgCode));
            Throw.IfNullOrWhiteSpace(learnDirectClassSystemCode1, nameof(learnDirectClassSystemCode1));
            Throw.IfNullOrWhiteSpace(learnDirectClassSystemCode2, nameof(learnDirectClassSystemCode2));
            Throw.IfNullOrWhiteSpace(guidedLearningHours, nameof(guidedLearningHours));
            //Throw.IfNullOrWhiteSpace(totalQualificationTime, nameof(totalQualificationTime));
            //Throw.IfNullOrWhiteSpace(unitType, nameof(unitType));
            Throw.IfNullOrWhiteSpace(awardOrgName, nameof(awardOrgName));
        }
    }
}