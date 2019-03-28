using System;
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
        public string LearnAimRefTypeDesc { get; }
        public DateTime? CertificationEndDate { get; }
        public string SectorSubjectAreaTier1Desc { get; }
        public string SectorSubjectAreaTier2Desc { get; }
        public string AwardOrgAimRef { get; }


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
            string awardOrgName,
            string learnAimRefTypeDesc,
            DateTime? certificationEndDate,
            string sectorSubjectAreaTier1Desc,
            string sectorSubjectAreaTier2Desc,
            string awardOrgAimRef)
        {
            Throw.IfLessThan(0, searchScore, nameof(searchScore));
            Throw.IfNullOrWhiteSpace(learnAimRef, nameof(learnAimRef));
            Throw.IfNullOrWhiteSpace(notionalNVQLevelv2, nameof(notionalNVQLevelv2));
            Throw.IfNullOrWhiteSpace(awardOrgCode, nameof(awardOrgCode));
            Throw.IfNullOrWhiteSpace(learnDirectClassSystemCode1, nameof(learnDirectClassSystemCode1));
            Throw.IfNullOrWhiteSpace(learnDirectClassSystemCode2, nameof(learnDirectClassSystemCode2));
            Throw.IfNullOrWhiteSpace(unitType, nameof(unitType));
            Throw.IfNullOrWhiteSpace(awardOrgName, nameof(awardOrgName));
            Throw.IfNullOrWhiteSpace(learnAimRefTypeDesc, nameof(learnAimRefTypeDesc));
            Throw.IfNullOrWhiteSpace(sectorSubjectAreaTier1Desc, nameof(sectorSubjectAreaTier1Desc));
            Throw.IfNullOrWhiteSpace(sectorSubjectAreaTier2Desc, nameof(sectorSubjectAreaTier2Desc));
            Throw.IfNullOrWhiteSpace(awardOrgAimRef, nameof(awardOrgAimRef));

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
            LearnAimRefTypeDesc = learnAimRefTypeDesc;
            CertificationEndDate = certificationEndDate;
            SectorSubjectAreaTier1Desc = sectorSubjectAreaTier1Desc;
            SectorSubjectAreaTier2Desc = sectorSubjectAreaTier2Desc;
            AwardOrgAimRef = awardOrgAimRef;
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
            yield return LearnAimRefTypeDesc;
            yield return CertificationEndDate;
            yield return SectorSubjectAreaTier1Desc;
            yield return SectorSubjectAreaTier2Desc;
            yield return AwardOrgAimRef;
        }
    }
}