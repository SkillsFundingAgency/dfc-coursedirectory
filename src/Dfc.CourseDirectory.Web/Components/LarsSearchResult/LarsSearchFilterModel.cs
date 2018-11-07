using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.Components.LarsSearchResult
{
    public class LarsSearchFilterModel
    {
        public string Title { get; set; }
        public IEnumerable<LarsFilterItemModel> Items { get; set; }
    }
}