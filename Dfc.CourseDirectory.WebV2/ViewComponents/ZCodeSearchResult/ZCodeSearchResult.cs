using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.ZCodeSearchResult
{
    public class ZCodeSearchResult : ViewComponent
    {
        public IViewComponentResult Invoke(ZCodeSearchResultModel model)
        {
            return View("~/ViewComponents/ZCodeSearchResult/Default.cshtml", model ?? new ZCodeSearchResultModel());
        }
    }
}
