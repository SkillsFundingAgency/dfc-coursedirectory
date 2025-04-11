using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.EntryRequirements
{
    public class EntryRequirements : ViewComponent
    {
        public IViewComponentResult Invoke(EntryRequirementsModel model)
        {
            return View("~/ViewComponents/Courses/EntryRequirements/Default.cshtml", model);
        }
    }
}
