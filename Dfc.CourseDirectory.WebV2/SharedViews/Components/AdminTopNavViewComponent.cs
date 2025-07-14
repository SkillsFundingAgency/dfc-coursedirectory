using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class AdminTopNavViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke() =>
            View("~/SharedViews/Components/AdminTopNav.cshtml");
    }
}
