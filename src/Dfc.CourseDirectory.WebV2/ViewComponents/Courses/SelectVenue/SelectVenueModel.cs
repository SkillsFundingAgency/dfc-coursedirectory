using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.Courses.SelectVenue
{
    public class SelectVenueModel
    {
        public List<VenueItemModel> VenueItems { get; set; }
        public int Ukprn { get; set; }

        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
