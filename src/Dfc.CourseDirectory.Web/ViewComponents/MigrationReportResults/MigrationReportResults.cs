using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.MigrationReportResults
{
    public class MigrationReportResults : ViewComponent
    {
        public IViewComponentResult Invoke(MigrationReportResultsModel model)
        {
            var actualModel = model ?? new MigrationReportResultsModel();

            return View("~/ViewComponents/MigrationReportResults/Default.cshtml", actualModel);
        }
    }
}
