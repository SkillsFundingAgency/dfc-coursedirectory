using Dfc.CourseDirectory.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services
{
    public class LarsSearchResultItem : ILarsSearchResultItem
    {
        public decimal SearchScore { get; set; }

        public string LearnAimRef { get; set; }

        public string LearnAimRefTitle { get; set; }

        public string NotionalNVQLevelv2 { get; set; }

        public string AwardOrgCode { get; set; }

        public string LearnDirectClassSystemCode1 { get; set; }

        public string LearnDirectClassSystemCode2 { get; set; }

        public string SectorSubjectAreaTier1 { get; set; }

        public string SectorSubjectAreaTier2 { get; set; }

        public string GuidedLearningHours { get; set; }

        public string TotalQualificationTime { get; set; }

        public string UnitType { get; set; }

        public string AwardOrgName { get; set; }
    }
}
