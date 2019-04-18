namespace Dfc.CourseDirectory.Web.RequestModels
{




    public class ProviderCoursesRequestModel
    {


        public string Keyword { get; set; }

        public string[] LevelFilter { get; set; }
        public string[] DeliveryModeFilter { get; set; }

        public string[] VenueFilter { get; set; }

        public string[] RegionFilter { get; set; }

        public string[] AttendancePatternFilter { get; set; }




        public int PageNo { get; set; }

        public ProviderCoursesRequestModel()
        {
            LevelFilter = new string[] { };
            DeliveryModeFilter = new string[] { };
            VenueFilter = new string[] { };
            RegionFilter = new string[] { };
            AttendancePatternFilter = new string[] { };

        }
    }
}