using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.Courses.WhereNext
{
    public class WhereNext : ViewComponent
    {
        public IViewComponentResult Invoke(WhereNextModel model)
        {
            return View("~/ViewComponents/Courses/WhereNext/Default.cshtml", model);
        }
    }
}
