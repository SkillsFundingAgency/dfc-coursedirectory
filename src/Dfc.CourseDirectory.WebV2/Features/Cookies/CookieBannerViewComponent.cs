using Dfc.CourseDirectory.WebV2.Cookies;
using Dfc.CourseDirectory.WebV2.Features.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class CookieBannerViewComponent : ViewComponent
    {
        private readonly ICookieSettingsProvider _cookieSettingsProvider;

        public CookieBannerViewComponent(ICookieSettingsProvider cookieSettingsProvider)
        {
            _cookieSettingsProvider = cookieSettingsProvider;
        }

        public IViewComponentResult Invoke()
        {
            var viewModel = new CookieBannerViewModel()
            {
                ShowBanner = _cookieSettingsProvider.GetPreferencesForCurrentUser() == null
            };

            return View("~/Features/Cookies/CookieBanner.cshtml", viewModel);
        }
    }
}
