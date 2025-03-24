using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.AddStartDate
{
    public class AddStartDate : ViewComponent
    {
        public IViewComponentResult Invoke(AddStartDateModel model)
        {
            return View("~/ViewComponents/Courses/AddStartDate/Default.cshtml", model);
        }
    }
}
