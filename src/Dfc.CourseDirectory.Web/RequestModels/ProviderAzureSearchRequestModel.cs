
namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class ProviderAzureSearchRequestModel
    {
        //public string APIKeyField { get; set; }
        public string Keyword { get; set; }
        //public string[] TownFilter { get; set; }
        public string[] Town { get; set; }
        //public string[] RegionFilter { get; set; }
        public string[] Region { get; set; }
        public int? TopResults { get; set; }
        //public int PageNo { get; set; }

        public ProviderAzureSearchRequestModel()
        {
            Town = new string[] { };
            Region = new string[] { };
            //PageNo = 1;
        }
    }
}
