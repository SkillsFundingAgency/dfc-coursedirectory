namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class LarsSearchRequestModel
    {
        public string SearchTerm { get; set; }
        public string[] AwardOrgCodeFilter { get; set; }
        public string[] NotionalNVQLevelv2Filter { get; set; }
        public int PageNo { get; set; }

        public LarsSearchRequestModel()
        {
            AwardOrgCodeFilter = new string[] { };
            NotionalNVQLevelv2Filter = new string[] { };
            PageNo = 1;
        }
    }
}