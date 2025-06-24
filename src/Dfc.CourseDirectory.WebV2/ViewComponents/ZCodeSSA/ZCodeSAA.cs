using Dfc.CourseDirectory.WebV2.ViewComponents.ZCodeSearchResult;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.ZCodeSSA
{
    public class ZCodeSSA : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
          

            return View("~/ViewComponents/ZCodeSSA/Default.cshtml", new ZCodeSearchResultModel());
        }
    }
}
