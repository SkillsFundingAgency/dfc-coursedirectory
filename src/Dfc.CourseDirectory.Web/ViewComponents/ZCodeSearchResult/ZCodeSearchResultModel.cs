using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult;

namespace Dfc.CourseDirectory.Web.ViewComponents.ZCodeSearchResult
{
    public class ZCodeSearchResultModel
    {
        public IEnumerable<ZCodeSearchResultItemModel> Items { get; set; }

        public string Url { get; set; }

        public string PageParamName { get; set; }

        public int ItemsPerPage { get; set; }

        public int TotalCount { get; set; }

        public IEnumerable<LarsSearchFilterModel> Filters { get; set; }

        public string Level1Id { get; set; }

        public string Level2Id { get; set; }

        public int CurrentPage { get; set; }

        public bool HasSelectedFilters => Filters.SelectMany(x => x.Items).Any(x => x.IsSelected);
    }
}