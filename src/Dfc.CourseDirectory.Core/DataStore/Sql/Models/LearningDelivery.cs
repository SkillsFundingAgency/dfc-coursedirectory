using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class LearningDelivery
    {
        public string LearnAimRef { get; set; }
        public string LearnAimRefTitle { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string AwardOrgCode { get; set; }
        public string LearnAimRefTypeDesc { get; set; }
        public string OperationalEndDate { get; set; }
    }
}
