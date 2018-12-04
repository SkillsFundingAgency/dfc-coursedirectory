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

        public Qualification(
            string notionalNVQLevelv2,
            string awardOrgCode,
            string totalQualificationTime,
            string unitType,
            string awardOrgName)
        {
            Throw.IfNullOrWhiteSpace(notionalNVQLevelv2, nameof(notionalNVQLevelv2));
            Throw.IfNullOrWhiteSpace(awardOrgCode, nameof(awardOrgCode));
            Throw.IfNullOrWhiteSpace(totalQualificationTime, nameof(totalQualificationTime));
            Throw.IfNullOrWhiteSpace(unitType, nameof(unitType));
            Throw.IfNullOrWhiteSpace(awardOrgName, nameof(awardOrgName));

            NotionalNVQLevelv2 = notionalNVQLevelv2;
            AwardOrgCode = awardOrgCode;
            TotalQualificationTime = totalQualificationTime;
            UnitType = unitType;
            AwardOrgCode = awardOrgCode;
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return NotionalNVQLevelv2;
            yield return AwardOrgCode;
            yield return TotalQualificationTime;
            yield return UnitType;
            yield return AwardOrgName;
        }

    }
}
