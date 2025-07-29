using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.HowAssessed
{
    public class HowAssessed : ViewComponent
    {
        public IViewComponentResult Invoke(HowAssessedModel model)
        {
            return View("~/ViewComponents/Courses/HowAssessed/Default.cshtml", model);
        }
    }
}
