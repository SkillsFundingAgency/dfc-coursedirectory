using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.UrlInput
{
    public class UrlInput : ViewComponent
    {
        public IViewComponentResult Invoke(UrlInputModel model)
        {
            return View("~/ViewComponents/UrlInput/Default.cshtml", model);
        }
    }
}
