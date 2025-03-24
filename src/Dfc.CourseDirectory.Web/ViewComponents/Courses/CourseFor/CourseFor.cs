using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseFor
{
    public class CourseFor : ViewComponent
    {
        public IViewComponentResult Invoke(CourseForModel model)
        {
            return View("~/ViewComponents/Courses/CourseFor/Default.cshtml", model);
        }
    }
}
