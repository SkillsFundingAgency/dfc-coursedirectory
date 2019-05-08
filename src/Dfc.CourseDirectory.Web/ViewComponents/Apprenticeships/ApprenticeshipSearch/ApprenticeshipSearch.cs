using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships.ApprenticeshipSearch
{
    public class ApprenticeshipSearch : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new ApprenticeshipSearchModel();

            return View("~/ViewComponents/Apprenticeships/ApprenticeshipSearch/Default.cshtml", model);
        }
    }
}