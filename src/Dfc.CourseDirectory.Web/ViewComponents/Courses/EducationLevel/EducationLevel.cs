using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.EducationLevel
{
    public class EducationLevel : ViewComponent
    {
        public IViewComponentResult Invoke(EducationLevelModel model)
        {
            return View("~/ViewComponents/Courses/EducationLevel/Default.cshtml", model);
        }
    }
}
