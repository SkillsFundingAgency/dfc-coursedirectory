using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Web.ViewComponents.MigrationReportResults;
using Dfc.CourseDirectory.Web.ViewComponents.MigrationReportDashboardPanel;
using System;

namespace Dfc.CourseDirectory.Web.ViewModels.Report
{
    public class MigrationReportViewModel2
    {
        public MigrationReportDashboardPanelModel TotalProvidersMigrated { get; set; }
        public MigrationReportDashboardPanelModel CoursesMigrated { get; set; }
        public MigrationReportDashboardPanelModel CoursesPending { get; set; }
        public MigrationReportDashboardPanelModel CoursesLive { get; set; }
        public MigrationReportDashboardPanelModel ApprenticeshipMigrated { get; set; }
        public MigrationReportDashboardPanelModel ApprenticeshipPending { get; set; }
        public MigrationReportDashboardPanelModel ApprenticeshipLive { get; set; }

        public string DateLastUpdated { get; set; }

        public MigrationReportResultsModel CourseReportResults { get; set; }
        public MigrationReportResultsModel ApprenticeshipReportResults { get; set; }

        public ReportType ReportType { get; set; }
    }
}
