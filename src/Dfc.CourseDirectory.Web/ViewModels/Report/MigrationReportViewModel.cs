using Dfc.CourseDirectory.Web.ViewComponents.MigrationReportResults;
using Dfc.CourseDirectory.Web.ViewComponents.MigrationReportDashboardPanel;

namespace Dfc.CourseDirectory.Web.ViewModels.Report
{
    public class MigrationReportViewModel
    {
        public MigrationReportDashboardPanelModel FEProvidersMigrated { get; set; }
        public MigrationReportDashboardPanelModel FECoursesMigrated { get; set; }
        public MigrationReportDashboardPanelModel FECoursesMigratedWithErrors { get; set; }
        public MigrationReportDashboardPanelModel CoursesLive { get; set; }

        public MigrationReportResultsModel ReportResults { get; set; }
    }
}
