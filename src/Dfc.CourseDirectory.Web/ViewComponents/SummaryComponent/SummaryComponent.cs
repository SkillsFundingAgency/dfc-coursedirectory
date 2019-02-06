using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.SummaryComponent
{
    public class SummaryComponent : ViewComponent
    {
        public IViewComponentResult Invoke(SummaryComponentModel model)
        {
            return View("~/ViewComponents/SummaryComponent/Default.cshtml", model);
        }
    }
}
