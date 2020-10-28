namespace Dfc.ProviderPortal.FindACourse.ApiModels
{
    public class LARSSearchRequest
    {
        public string Keyword { get; set; }
        public string[] AwardOrgCode { get; set; }
        public string[] NotionalNVQLevelv2 { get; set; }
        public string[] SectorSubjectAreaTier1 { get; set; }
        public string[] SectorSubjectAreaTier2 { get; set; }
        public string[] AwardOrgAimRef { get; set; }
        public int? TopResults { get; set; }
    }
}
