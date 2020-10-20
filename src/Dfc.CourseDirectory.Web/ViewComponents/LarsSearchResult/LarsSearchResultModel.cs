using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;

namespace Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult
{
    public class LarsSearchResultModel : IViewComponentModel
    {
        public LarsSearchResultModel()
        {
            Items = Enumerable.Empty<LarsSearchResultItemModel>();
            Filters = Enumerable.Empty<LarsSearchFilterModel>();
            Errors = Enumerable.Empty<string>();
        }

        public string SearchTerm { get; set; }
        
        public IEnumerable<LarsSearchResultItemModel> Items { get; set; }
        
        public string Url { get; set; }
        
        public string PageParamName { get; set; }
        
        public int PageNumber { get; set; }
        
        public int ItemsPerPage { get; set; }
        
        public IEnumerable<LarsSearchFilterModel> Filters { get; set; }

        public int TotalCount { get; set;  }

        public IEnumerable<string> Errors { get; }

        public bool HasSelectedFilters => Filters.SelectMany(x => x.Items).Any(x => x.IsSelected);

        public bool HasErrors => Errors.Count() > 0;
    }
}