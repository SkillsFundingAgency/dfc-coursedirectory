using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Components.LarsSearchResult
{
    public class LarsSearchResult : ViewComponent
    {
        public IViewComponentResult Invoke(LarsSearchResultModel model)
        {
            var actualModel = model ?? new LarsSearchResultModel();

            return View("~/Components/LarsSearchResult/Default.cshtml", actualModel);
        }
    }
}