using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.BackLink
{
    public class BackLink : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/ViewComponents/BackLink/Default.cshtml");
        }
    }
}
