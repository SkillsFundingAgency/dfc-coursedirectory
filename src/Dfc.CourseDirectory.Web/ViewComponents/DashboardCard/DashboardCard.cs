using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.DashboardCard
{
    public class DashboardCard : ViewComponent
    {
        public IViewComponentResult Invoke(DashboardCardModel model)
        {
            return View("~/ViewComponents/DashboardCard/Default.cshtml", model);
        }
    }
}
