using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult
{
    public class LarsSearchResult : ViewComponent
    {
        public IViewComponentResult Invoke(LarsSearchResultModel model)
        {
            var actualModel = model ?? new LarsSearchResultModel();

            return View("~/ViewComponents/LarsSearchResult/Default.cshtml", actualModel);
        }
    }
}