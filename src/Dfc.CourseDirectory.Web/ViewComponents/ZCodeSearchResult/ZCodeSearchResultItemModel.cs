using Dfc.CourseDirectory.Core.Search.Models;

namespace Dfc.CourseDirectory.Web.ViewComponents.ZCodeSearchResult
{
    public class ZCodeSearchResultItemModel
    {
        public string LearnAimRef { get; set; }

        public string LearnAimRefTitle { get; set; }

        public string NotionalNVQLevelv2 { get; set; }

        public string AwardOrgCode { get; set; }

        public string LearnDirectClassSystemCode1 { get; set; }

        public string LearnDirectClassSystemCode2 { get; set; }

        public string GuidedLearningHours { get; set; }

        public string TotalQualificationTime { get; set; }

        public string UnitType { get; set; }

        public string AwardOrgName { get; set; }

        public string LearnAimRefTypeDesc { get; set; }

        public static ZCodeSearchResultItemModel FromLars(Lars lars)
        {
            return new ZCodeSearchResultItemModel
            {
                LearnAimRef = lars.LearnAimRef,
                LearnAimRefTitle = lars.LearnAimRefTitle,
                NotionalNVQLevelv2 = lars.NotionalNVQLevelv2,
                AwardOrgCode = lars.AwardOrgCode,
                LearnDirectClassSystemCode1 = lars.LearnDirectClassSystemCode1,
                LearnDirectClassSystemCode2 = lars.LearnDirectClassSystemCode2,
                GuidedLearningHours = lars.GuidedLearningHours,
                TotalQualificationTime = lars.TotalQualificationTime,
                UnitType = lars.UnitType,
                AwardOrgName = lars.AwardOrgName,
                LearnAimRefTypeDesc = lars.LearnAimRefTypeDesc
            };
        }
    }
}