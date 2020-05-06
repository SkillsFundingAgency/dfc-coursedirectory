using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;
using Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Web.ViewComponents.ZCodeSearchResult
{
    public class ZCodeSearchResultModel {
        public IEnumerable<ZCodeSearchResultItemModel> Items { get; set; }

        public string Url { get; set; }
        public string PageParamName { get; set; }
        public int ItemsPerPage { get; set; }

        public int TotalCount { get; set; }

        public List<LarsSearchFilterModel> Filters { get; set; }

        public string Level1Id { get; set; }

        public string Level2Id { get; set; }

        public string filter0Id { get; set; }

        public string filter1Id { get; set; }

        public bool HasSelectedFilters => Filters.SelectMany(x => x.Items).Any(x => x.IsSelected);
        public int CurrentPage { get; set; }
    }
}