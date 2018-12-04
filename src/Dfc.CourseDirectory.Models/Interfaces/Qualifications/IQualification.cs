using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Qualifications
{
    public interface IQualification
    {
        string NotionalNVQLevelv2 { get; }
        string AwardOrgCode { get; }
        string TotalQualificationTime { get; }
        string UnitType { get; }
        string AwardOrgName { get; }
    }
}
