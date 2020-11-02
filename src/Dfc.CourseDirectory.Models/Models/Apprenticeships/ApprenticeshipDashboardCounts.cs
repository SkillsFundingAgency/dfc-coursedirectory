using System;

namespace Dfc.CourseDirectory.Models.Models.Apprenticeships
{
    public class ApprenticeshipDashboardCounts
    {
        public int? PublishedApprenticeshipCount { get; set; }
        public int? BulkUploadPendingCount { get; set; }
        public int? BulkUploadReadyToGoLiveCount { get; set; }
        public int? BulkUploadTotalCount { get; set; }
        public int? TotalErrors { get; set; }
        public DateTimeOffset? FileUploadDate { get; set; }
    }
}
