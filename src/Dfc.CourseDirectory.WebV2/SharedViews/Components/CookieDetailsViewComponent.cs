using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class CookieDetailsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke() =>  View("~/SharedViews/Components/CookieDetails.cshtml");
    }
}
