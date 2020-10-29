
using System;
using Dfc.ProviderPortal.Packages;


namespace Dfc.CourseDirectory.FindACourseApi.Models
{
    public class LARSSearchCriteriaStructure
    {
        //public string APIKeyField { get; set; }
        public string Keyword { get; set; }
        public string[] AwardOrgCode { get; set; }
        public string[] NotionalNVQLevelv2 { get; set; }
        public string[] SectorSubjectAreaTier1 { get; set; }
        public string[] SectorSubjectAreaTier2 { get; set; }
        public string[] AwardOrgAimRef { get; set; }
        public int? TopResults { get; set; }

        public LARSSearchCriteriaStructure()
        {
            //if (TopResults.HasValue)
            //    Throw.IfLessThan(1, TopResults.Value, "");
        }
    }
}
