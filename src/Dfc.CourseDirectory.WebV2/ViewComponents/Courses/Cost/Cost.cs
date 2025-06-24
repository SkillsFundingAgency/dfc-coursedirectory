using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.Courses.Cost
{
    public class Cost : ViewComponent
    {
        public IViewComponentResult Invoke(CostModel model)
        {
            return View("~/ViewComponents/Courses/Cost/Default.cshtml", model);
        }
    }
}
