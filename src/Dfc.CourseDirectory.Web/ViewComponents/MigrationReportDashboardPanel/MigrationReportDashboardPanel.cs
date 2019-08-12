using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.MigrationReportDashboardPanel
{
    public class MigrationReportDashboardPanel : ViewComponent
    {
        public IViewComponentResult Invoke(MigrationReportDashboardPanelModel model)
        {
            return View("~/ViewComponents/MigrationReportDashboardPanel/Default.cshtml", model);
        }
    }
}
