using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult
{
    public class LarsSearchResultItemModel : ValueObject<LarsSearchResultItemModel>, IViewComponentModel
    {
        public bool HasErrors => Errors.Count() > 0;
        public IEnumerable<string> Errors { get; }
        public decimal SearchScore { get; }
        public string LearnAimRef { get; }
        public string LearnAimRefTitle { get; }
        public string NotionalNVQLevelv2 { get; }
        public string AwardOrgCode { get; }
        public string LearnDirectClassSystemCode1 { get; }
        public string LearnDirectClassSystemCode2 { get; }
        public string GuidedLearningHours { get; }
        public string TotalQualificationTime { get; }
        public string UnitType { get; }
        public string AwardOrgName { get; }
        public string LearnAimRefTypeDesc { get; }

        public LarsSearchResultItemModel(
            decimal searchScore,
            string learnAimRef,
            string learnAimRefTitle,
            string notionalNVQLevelv2,
            string awardOrgCode,
            string learnDirectClassSystemCode1,
            string learnDirectClassSystemCode2,
            string guidedLearningHours,
            string totalQualificationTime,
            string unitType,
            string awardOrgName,
            string learnAimRefTypeDesc)
        {
            Errors = new string[] { };
            SearchScore = searchScore;
            LearnAimRef = learnAimRef;
            LearnAimRefTitle = learnAimRefTitle;
            NotionalNVQLevelv2 = notionalNVQLevelv2;
            AwardOrgCode = awardOrgCode;
            LearnDirectClassSystemCode1 = learnDirectClassSystemCode1;
            LearnDirectClassSystemCode2 = learnDirectClassSystemCode2;
            GuidedLearningHours = guidedLearningHours;
            TotalQualificationTime = totalQualificationTime;
            UnitType = unitType;
            AwardOrgName = awardOrgName;
            LearnAimRefTypeDesc = learnAimRefTypeDesc;
        }

        public LarsSearchResultItemModel()
        {
            Errors = new string[] { };
        }

        public LarsSearchResultItemModel(string error)
        {
            Errors = new string[] { error };
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HasErrors;
            yield return Errors;
            yield return SearchScore;
            yield return LearnAimRef;
            yield return LearnAimRefTitle;
            yield return NotionalNVQLevelv2;
            yield return AwardOrgCode;
            yield return LearnDirectClassSystemCode1;
            yield return LearnDirectClassSystemCode2;
            yield return GuidedLearningHours;
            yield return TotalQualificationTime;
            yield return UnitType;
            yield return AwardOrgName;
            yield return LearnAimRefTypeDesc;
        }
    }
}