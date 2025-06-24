using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.CourseProviderReference
{
    public class CourseProviderReference : ViewComponent
    {
        public IViewComponentResult Invoke(CourseProviderReferenceModel model)
        {
            return View("~/ViewComponents/CourseProviderReference/Default.cshtml", model);
        }
    }
}
