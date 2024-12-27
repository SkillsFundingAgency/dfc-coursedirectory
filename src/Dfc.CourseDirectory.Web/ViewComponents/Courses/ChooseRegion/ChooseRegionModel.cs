using Dfc.CourseDirectory.Services.Models.Regions;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseRegion
{
    public class ChooseRegionModel
    {
        public bool UseNationalComponent { get; set; } = true;
        public bool? National { get; set; }
        public SelectRegionModel Regions { get; set; }
        public bool HasOtherDeliveryOptions { get; set; }

    }
}
