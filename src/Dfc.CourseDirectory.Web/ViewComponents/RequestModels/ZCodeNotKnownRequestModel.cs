namespace Dfc.CourseDirectory.Web.RequestModels
{
    //public class ZCodeNotKnownRequestModel
    //{
    //    public string Level1Id { get; set; }
    //    public string Level2Id { get; set; }

    //    public string LevelId { get; set; }

    //    public string CategoryId { get; set; }
    //}




    public class ZCodeNotKnownRequestModel
    {
        public string Level1Id { get; set; }
        public string Level2Id { get; set; }

        //public string LevelId { get; set; }

        //public string CategoryId { get; set; }
        //public string SearchTerm { get; set; }
        //public string[] AwardOrgCodeFilter { get; set; }
        public string[] NotionalNVQLevelv2Filter { get; set; }
        //public string[] SectorSubjectAreaTier1Filter { get; set; }
        //public string[] SectorSubjectAreaTier2Filter { get; set; }
        public string[] AwardOrgAimRefFilter { get; set; }


        public int PageNo { get; set; }

        public ZCodeNotKnownRequestModel()
        {
            //AwardOrgCodeFilter = new string[] { };
            NotionalNVQLevelv2Filter = new string[] { };
           // SectorSubjectAreaTier1Filter = new string[] { };
            //SectorSubjectAreaTier2Filter = new string[] { };
            AwardOrgAimRefFilter = new string[] { };
            PageNo = 1;
        }
    }
}