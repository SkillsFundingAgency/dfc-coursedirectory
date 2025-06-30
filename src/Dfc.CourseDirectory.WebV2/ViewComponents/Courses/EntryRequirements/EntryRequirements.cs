using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.Courses.EntryRequirements
{
    public class EntryRequirements : ViewComponent
    {
        public IViewComponentResult Invoke(EntryRequirementsModel model)
        {
            return View("~/ViewComponents/Courses/EntryRequirements/Default.cshtml", model);
        }
    }
}
