using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.Cost
{
    public class Cost : ViewComponent
    {
        public IViewComponentResult Invoke(CostModel model)
        {
            return View("~/ViewComponents/Courses/Cost/Default.cshtml", model);
        }
    }
}
