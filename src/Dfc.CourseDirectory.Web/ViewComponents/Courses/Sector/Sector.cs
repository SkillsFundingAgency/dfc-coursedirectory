using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.Sector
{
    public class Sector : ViewComponent
    {
        public IViewComponentResult Invoke(SectorModel model)
        {
            return View("~/ViewComponents/Courses/Sector/Default.cshtml", model);
        }
    }
}
