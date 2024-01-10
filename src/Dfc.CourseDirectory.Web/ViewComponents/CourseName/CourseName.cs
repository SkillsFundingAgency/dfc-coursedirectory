using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.CourseName
{
    public class CourseName : ViewComponent
    {
        public IViewComponentResult Invoke(CourseNameModel model)
        {
            return View("~/ViewComponents/CourseName/Default.cshtml", model);
        }
    }
}
