using Dfc.CourseDirectory.Web.ViewModels.SearchProvider;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.SearchProviderResults
{
    public class SearchProviderResults : ViewComponent
    {
        public IViewComponentResult Invoke(SearchProviderResultsViewModel model)
        {
            return View("~/ViewComponents/SearchProviderResults/Default.cshtml", model ?? new SearchProviderResultsViewModel());
        }
    }
}