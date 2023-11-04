using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseType
{
    public class CourseType : ViewComponent
    {
        public IViewComponentResult Invoke(CourseTypeModel model)
        {
            return View("~/ViewComponents/Courses/CourseType/Default.cshtml", model);
        }
    }
}
