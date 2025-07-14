using Dfc.CourseDirectory.WebV2.ViewModels.ProviderCourses;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.ProviderCoursesResults
{
    public class ProviderCoursesResults : ViewComponent
    {
        public IViewComponentResult Invoke(ProviderCoursesViewModel model)
        {
            var actualModel = model ?? new ProviderCoursesViewModel();
            return View("~/ViewComponents/ProviderCoursesResults/Default.cshtml", actualModel);
        }
    }
}
