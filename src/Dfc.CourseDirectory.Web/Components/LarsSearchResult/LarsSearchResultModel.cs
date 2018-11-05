using Dfc.CourseDirectory.Common;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.Components.LarsSearchResult
{
    public class LarsSearchResultModel : ViewComponentModel
    {
        public string SearchTerm { get; set; }
        public IEnumerable<LarsSearchResultItemModel> Items { get; set; }

        public LarsSearchResultModel() : base()
        {
            Items = new List<LarsSearchResultItemModel>();
        }

        public LarsSearchResultModel(string error) : base(new string[] { error })
        {
            Items = new List<LarsSearchResultItemModel>();
        }

        public LarsSearchResultModel(
            string searchTerm,
            IEnumerable<LarsSearchResultItemModel> items)
        {
            Throw.IfNullOrWhiteSpace(searchTerm, nameof(searchTerm));
            Throw.IfNull(items, nameof(items));

            SearchTerm = searchTerm;
            Items = items;
        }
    }
}