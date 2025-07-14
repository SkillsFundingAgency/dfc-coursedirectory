using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.Courses.AwardingBody
{
    public class AwardingBody : ViewComponent
    {
        public IViewComponentResult Invoke(AwardingBodyModel model)
        {
            return View("~/ViewComponents/Courses/AwardingBody/Default.cshtml", model);
        }
    }
}
