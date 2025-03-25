using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.StudyMode
{
    public class StudyMode : ViewComponent
    {
        public IViewComponentResult Invoke(StudyModeModel model)
        {
            return View("~/ViewComponents/Courses/StudyMode/Default.cshtml", model);
        }
    }
}
