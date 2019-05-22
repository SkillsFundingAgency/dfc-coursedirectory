using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.ProviderApprenticeships.ProviderApprenticeshipSearch
{
    public class ProviderApprenticeshipsSearch : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new ProviderApprenticeshipsSearchModel();

            return View("~/ViewComponents/ProviderApprenticeships/ProviderApprenticeshipsSearch/Default.cshtml", model);
        }
    }
}