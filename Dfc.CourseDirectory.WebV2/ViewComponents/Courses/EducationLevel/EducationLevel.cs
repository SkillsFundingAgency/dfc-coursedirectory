using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.Courses.EducationLevel
{
    public class EducationLevel : ViewComponent
    {
        public IViewComponentResult Invoke(EducationLevelModel model)
        {
            return View("~/ViewComponents/Courses/EducationLevel/Default.cshtml", model);
        }
    }
}
