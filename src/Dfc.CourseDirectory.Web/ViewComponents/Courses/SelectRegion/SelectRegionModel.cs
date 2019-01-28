using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.SelectRegion
{
    public class SelectRegionModel
    {
        public IEnumerable<RegionItemModel> RegionItems { get; set; }

        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }

        public SelectRegionModel()
        {
            RegionItems = new List<RegionItemModel>();
        }
    }
}