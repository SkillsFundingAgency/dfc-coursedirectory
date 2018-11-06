using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Components.LarsSearchResult
{
    public class LarsSearchResult : ViewComponent
    {
        public IViewComponentResult Invoke(LarsSearchResultModel model)
        {
            return View("~/Components/LarsSearchResult/Default.cshtml", model);
        }
    }
}