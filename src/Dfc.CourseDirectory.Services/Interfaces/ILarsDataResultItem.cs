using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface ILarsDataResultItem
    {
        string LearnAimRef { get; set; }
        string LearnAimRefTitle { get; set; }
        string NotionalNVQLevelv2 { get; set; }
        string AwardOrgCode { get; set; }
        string LearnAimRefTypeDesc { get; set; }
        DateTime? CertificationEndDate { get; set; }
    }
}
