using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.Courses.HowYouWillLearn
{
    public class HowYouWillLearn : ViewComponent
    {
        public IViewComponentResult Invoke(HowYouWillLearnModel model)
        {
            return View("~/ViewComponents/Courses/HowYouWillLearn/Default.cshtml", model);
        }
    }
}
