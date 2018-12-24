using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Qualifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Qualifications
{
    public class Qualification : ValueObject<Qualification>, IQualification
    {
        public string NotionalNVQLevelv2 { get; }
        public string AwardOrgCode { get; }
        public string TotalQualificationTime { get; }
        public string UnitType { get; }
        public string AwardOrgName { get; }
        public string LearnAimRef { get; }
        public string LearnAimRefTitle { get; }
        public string LearnDirectClassSystemCode1 { get; }
        public string LearnDirectClassSystemCode2 { get; }
        public string GuidedLearningHours { get; }

        public Qualification(
            string notionalNVQLevelv2,
            string awardOrgCode,
            string totalQualificationTime,
            string unitType,
            string awardOrgName,
            string learnAimRef,
            string learnAimRefTitle,
            string learnDirectClassSystemCode1,
            string learnDirectClassSystemCode2,
            string guidedLearningHours)
        {
            Throw.IfNullOrWhiteSpace(notionalNVQLevelv2, nameof(notionalNVQLevelv2));
            Throw.IfNullOrWhiteSpace(awardOrgCode, nameof(awardOrgCode));
            Throw.IfNullOrWhiteSpace(totalQualificationTime, nameof(totalQualificationTime));
            Throw.IfNullOrWhiteSpace(unitType, nameof(unitType));
            Throw.IfNullOrWhiteSpace(awardOrgName, nameof(awardOrgName));
            Throw.IfNullOrWhiteSpace(learnAimRef, nameof(learnAimRef));
            Throw.IfNullOrWhiteSpace(learnAimRefTitle, nameof(learnAimRefTitle));
            Throw.IfNullOrWhiteSpace(learnDirectClassSystemCode1, nameof(learnDirectClassSystemCode1));
            Throw.IfNullOrWhiteSpace(learnDirectClassSystemCode2, nameof(learnDirectClassSystemCode2));
            Throw.IfNullOrWhiteSpace(guidedLearningHours, nameof(guidedLearningHours));

            NotionalNVQLevelv2 = notionalNVQLevelv2;
            AwardOrgCode = awardOrgCode;
            TotalQualificationTime = totalQualificationTime;
            UnitType = unitType;
            AwardOrgCode = awardOrgCode;
            LearnAimRef = learnAimRef;
            LearnAimRefTitle = learnAimRefTitle;
            LearnDirectClassSystemCode1 = learnDirectClassSystemCode1;
            LearnDirectClassSystemCode2 = learnDirectClassSystemCode2;
            GuidedLearningHours = guidedLearningHours;
    }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return NotionalNVQLevelv2;
            yield return AwardOrgCode;
            yield return TotalQualificationTime;
            yield return UnitType;
            yield return AwardOrgName;
            yield return LearnAimRef;
            yield return LearnAimRefTitle;
            yield return LearnDirectClassSystemCode1;
            yield return LearnDirectClassSystemCode2;
            yield return GuidedLearningHours;
        }

    }
}
