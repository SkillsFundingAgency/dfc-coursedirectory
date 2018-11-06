using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Components.LarsSearch
{
    public class LarsSearch : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new LarsSearchModel();

            return View("~/Components/LarsSearch/Default.cshtml", model);
        }
    }
}