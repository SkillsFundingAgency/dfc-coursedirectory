using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class AdminProviderContextNavViewComponent : ViewComponent
    {
        private readonly IProviderContextProvider _providerContextProvider;

        public AdminProviderContextNavViewComponent(IProviderContextProvider providerContextProvider)
        {
            _providerContextProvider = providerContextProvider;
        }

        public IViewComponentResult Invoke()
        {
            var providerContext = _providerContextProvider.GetProviderContext();
            return View("~/SharedViews/Components/AdminProviderContextNav.cshtml", providerContext);
        }
    }
}
