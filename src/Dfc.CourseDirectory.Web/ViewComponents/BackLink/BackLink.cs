using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.BackLink
{
    public class BackLink : ViewComponent
    {
        public IViewComponentResult Invoke(string controller = null, string action = null)
        {
            BackLinkModel model = new BackLinkModel(controller, action);
            return View("~/ViewComponents/BackLink/Default.cshtml", model);
        }
    }
}
