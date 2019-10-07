using Dfc.CourseDirectory.Web.ViewModels.PublishApprenticeships;
using Dfc.CourseDirectory.Web.ViewModels.PublishCourses;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.PublishCourses.Publish
{
    public class Publish : ViewComponent
    {
        public IViewComponentResult Invoke(PublishViewModel model)
        {
            return View("~/ViewComponents/PublishCourses/Publish/Default.cshtml", model);
        }
    }
}
