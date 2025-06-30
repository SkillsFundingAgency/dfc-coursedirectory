using Microsoft.AspNetCore.Mvc;
using Dfc.CourseDirectory.Core.Middleware;
namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class ProviderTopNavViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ProviderInfo providerInfo)
        {
            var vm = ProviderNavViewModel.Create(providerInfo);

            return View("~/SharedViews/Components/ProviderTopNav.cshtml", vm);
        }
    }
}
