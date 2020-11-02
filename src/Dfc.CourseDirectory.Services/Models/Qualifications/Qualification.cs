using System;

namespace Dfc.CourseDirectory.Services.Models.Qualifications
{
    public class Qualification
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
            if (string.IsNullOrWhiteSpace(notionalNVQLevelv2))
            {
                throw new ArgumentException("message", nameof(notionalNVQLevelv2));
            }

            if (string.IsNullOrWhiteSpace(awardOrgCode))
            {
                throw new ArgumentException("message", nameof(awardOrgCode));
            }

            if (string.IsNullOrWhiteSpace(totalQualificationTime))
            {
                throw new ArgumentException("message", nameof(totalQualificationTime));
            }

            if (string.IsNullOrWhiteSpace(unitType))
            {
                throw new ArgumentException("message", nameof(unitType));
            }

            if (string.IsNullOrEmpty(awardOrgName))
            {
                throw new ArgumentException("message", nameof(awardOrgName));
            }

            if (string.IsNullOrWhiteSpace(learnAimRef))
            {
                throw new ArgumentException("message", nameof(learnAimRef));
            }

            if (string.IsNullOrWhiteSpace(learnAimRefTitle))
            {
                throw new ArgumentException("message", nameof(learnAimRefTitle));
            }

            if (string.IsNullOrWhiteSpace(learnDirectClassSystemCode1))
            {
                throw new ArgumentException("message", nameof(learnDirectClassSystemCode1));
            }

            if (string.IsNullOrWhiteSpace(learnDirectClassSystemCode2))
            {
                throw new ArgumentException("message", nameof(learnDirectClassSystemCode2));
            }

            if (string.IsNullOrWhiteSpace(guidedLearningHours))
            {
                throw new ArgumentException("message", nameof(guidedLearningHours));
            }

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
    }
}
