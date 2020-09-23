using System;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class LarsSearchRequestModel
    {
        public LarsSearchRequestModel()
        {
            AwardOrgCodeFilter = Array.Empty<string>();
            NotionalNVQLevelv2Filter = Array.Empty<string>();
            SectorSubjectAreaTier1Filter = Array.Empty<string>();
            SectorSubjectAreaTier2Filter = Array.Empty<string>();
            AwardOrgAimRefFilter = Array.Empty<string>();
            PageNo = 1;
        }

        public string SearchTerm { get; set; }

        public string[] AwardOrgCodeFilter { get; set; }

        public string[] NotionalNVQLevelv2Filter { get; set; }

        public string[] SectorSubjectAreaTier1Filter { get; set; }

        public string[] SectorSubjectAreaTier2Filter { get; set; }

        public string[] AwardOrgAimRefFilter { get; set; }

        public int PageNo { get; set; }
    }
}