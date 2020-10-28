
using System;


namespace Dfc.ProviderPortal.FindACourse.Interfaces
{
    public interface IQualification
    {
        string NotionalNVQLevelv2 { get; }
        string AwardOrgCode { get; }
        string TotalQualificationTime { get; }
        string UnitType { get; }
        string AwardOrgName { get; }
        string LearnAimRef { get; }
        string LearnAimRefTitle { get; }
        string LearnDirectClassSystemCode1 { get; }
        string LearnDirectClassSystemCode2 { get; }
        string GuidedLearningHours { get; }
    }
}
