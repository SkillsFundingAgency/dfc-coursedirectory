using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewComponents.Summary.SummaryList
{
    public class SummaryListModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public IEnumerable<string> Value { get; set; }
        public string Href { get; set; }
    }
}
