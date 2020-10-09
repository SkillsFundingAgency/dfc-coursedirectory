using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Web.ViewModels.SearchProvider
{
    public class SearchProviderResultsViewModel
    {
        public string Search { get; set; }

        public IEnumerable<SearchProviderResultViewModel> Providers { get; set; }

        public static SearchProviderResultsViewModel Empty()
        {
            return new SearchProviderResultsViewModel
            {
                Search = string.Empty,
                Providers = Enumerable.Empty<SearchProviderResultViewModel>()
            };
        }
    }
}