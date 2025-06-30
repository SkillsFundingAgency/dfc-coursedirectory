using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.Courses.HowAssessed
{
    public class HowAssessed : ViewComponent
    {
        public IViewComponentResult Invoke(HowAssessedModel model)
        {
            return View("~/ViewComponents/Courses/HowAssessed/Default.cshtml", model);
        }
    }
}
