using Dfc.CourseDirectory.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services
{
    public class LarsDataResultItem : ILarsDataResultItem
    {
        public string LearnAimRef { get; set; }
        public string LearnAimRefTitle { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string AwardOrgCode { get; set; }
        public string LearnAimRefTypeDesc { get; set; }
        public DateTime? CertificationEndDate { get; set; }
    }
}
