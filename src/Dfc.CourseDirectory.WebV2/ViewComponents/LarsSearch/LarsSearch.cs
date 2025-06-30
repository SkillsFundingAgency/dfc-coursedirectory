using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.LarsSearch
{
    public class LarsSearch : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new LarsSearchModel();

            return View("~/ViewComponents/LarsSearch/Default.cshtml", model);
        }
    }
}
