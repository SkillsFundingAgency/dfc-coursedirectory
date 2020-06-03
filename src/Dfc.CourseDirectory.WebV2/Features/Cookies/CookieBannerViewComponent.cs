using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class CookieBannerViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke() => View("~/Features/Cookies/CookieBanner.cshtml");
    }
}
