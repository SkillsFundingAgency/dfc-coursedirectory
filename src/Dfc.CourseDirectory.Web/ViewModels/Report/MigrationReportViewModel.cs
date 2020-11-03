using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Web.ViewComponents.MigrationReportResults;
using Dfc.CourseDirectory.Web.ViewComponents.MigrationReportDashboardPanel;

namespace Dfc.CourseDirectory.Web.ViewModels.Report
{
    public class MigrationReportViewModel
    {
        public MigrationReportDashboardPanelModel ProvidersMigrated { get; set; }
        public MigrationReportDashboardPanelModel Migrated { get; set; }
        public MigrationReportDashboardPanelModel MigratedWithErrors { get; set; }
        public MigrationReportDashboardPanelModel Live { get; set; }

        public MigrationReportResultsModel ReportResults { get; set; }

        public ReportType ReportType { get; set; }
    }
}
