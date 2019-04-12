namespace Dfc.CourseDirectory.Web.RequestModels
{




    public class ProviderCoursesRequestModel
    {


        //public string LevelId { get; set; }

        //public string CategoryId { get; set; }
        //public string SearchTerm { get; set; }
        //public string[] AwardOrgCodeFilter { get; set; }
        public string[] LevelFilter { get; set; }
        public string[] DeliveryModeFilter { get; set; }
        //public string[] SectorSubjectAreaTier2Filter { get; set; }



        public int PageNo { get; set; }

        public ProviderCoursesRequestModel()
        {
            //AwardOrgCodeFilter = new string[] { };
            LevelFilter = new string[] { };
            DeliveryModeFilter = new string[] { };
            // SectorSubjectAreaTier1Filter = new string[] { };
            //SectorSubjectAreaTier2Filter = new string[] { };
            //AwardOrgAimRefFilter = new string[] { };
            PageNo = 1;
        }
    }
}