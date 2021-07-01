using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class SubRegionSelectorViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IReadOnlyCollection<string> SelectedSubRegionIds { get; set; }
    }
}
