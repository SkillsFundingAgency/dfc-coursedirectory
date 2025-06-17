using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.LarsSearchResult
{
    public class LarsSearchFilterModel
    {
        public string Title { get; set; }

        public IEnumerable<LarsSearchFilterItemModel> Items { get; set; }
    }
}
