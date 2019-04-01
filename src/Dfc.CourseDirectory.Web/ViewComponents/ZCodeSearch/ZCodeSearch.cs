using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.ZCodeSearch
{
    public class ZCodeSearch : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new ZCodeSearchModel();

            return View("~/ViewComponents/ZCodeSearch/Default.cshtml", model);
        }
    }
}