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

        public IEnumerable<LarsSearchFilterModel> Filters { get; set; }
    }
}