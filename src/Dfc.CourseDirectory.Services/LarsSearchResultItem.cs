using System.Collections.Generic;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces;

namespace Dfc.CourseDirectory.Services
{
    public class LarsSearchResultItem : ValueObject<LarsSearchResultItem>, ILarsSearchResultItem
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
            Throw.IfNullOrWhiteSpace(awardOrgCode, nameof(awardOrgCode));
            Throw.IfNullOrWhiteSpace(learnDirectClassSystemCode1, nameof(learnDirectClassSystemCode1));
            Throw.IfNullOrWhiteSpace(learnDirectClassSystemCode2, nameof(learnDirectClassSystemCode2));
            Throw.IfNullOrWhiteSpace(unitType, nameof(unitType));
            Throw.IfNullOrWhiteSpace(awardOrgName, nameof(awardOrgName));

            SearchScore = searchScore;
            LearnAimRef = learnAimRef;
            NotionalNVQLevelv2 = notionalNVQLevelv2;
            AwardOrgCode = awardOrgCode;
            LearnDirectClassSystemCode1 = learnDirectClassSystemCode1;
            LearnDirectClassSystemCode2 = learnDirectClassSystemCode2;
            GuidedLearningHours = guidedLearningHours;
            TotalQualificationTime = totalQualificationTime;
            UnitType = unitType;
            AwardOrgName = AwardOrgName;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return SearchScore;
            yield return LearnAimRef;
            yield return NotionalNVQLevelv2;
            yield return AwardOrgCode;
            yield return LearnDirectClassSystemCode1;
            yield return LearnDirectClassSystemCode2;
            yield return GuidedLearningHours;
            yield return TotalQualificationTime;
            yield return UnitType;
            yield return AwardOrgName;
        }
    }
}