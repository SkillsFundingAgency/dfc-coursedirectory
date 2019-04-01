using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.ZCodeSearchResult
{
    public class ZCodeSearchResult : ViewComponent
    {
        public IViewComponentResult Invoke(ZCodeSearchResultModel model)
        {
            var actualModel = model ?? new ZCodeSearchResultModel();

            return View("~/ViewComponents/ZCodeSearchResult/Default.cshtml", actualModel);
        }
    }
}