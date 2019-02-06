using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.SelectRegion;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.SelectVenue;

namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class AddCourseSummaryViewModel
    {
        public AddCourseSection1RequestModel Section1 { get; set; }
        public AddCourseRequestModel Section2 { get; set; }
        public SelectVenueModel Venues { get; set; }
        public SelectRegionModel Region { get; set; }

        public string CourseName { get; set; }
        public string CourseId { get; set; }
        public string DeliveryMode { get; set; }
        public string StartDate { get; set; }
        public string VenueList { get; set; }
        public string RegionList { get; set; }
    }
}
