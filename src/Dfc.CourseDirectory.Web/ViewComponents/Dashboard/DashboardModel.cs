using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Dashboard
{
    public class DashboardModel
    {
        public int PublishedCourseCount { get; set; }
        public int VenueCount { get; set; }

        public IEnumerable<string> ValidationMessages { get; set; }

        public DateTimeOffset? FileUploadDate { get; set; }

        public int FileCount { get; set; }

        public int BulkUploadPendingCount { get; set; }

        public int BulkUploadReadyToGoLiveCount { get; set; }

        public int BulkUploadTotalCount { get; set; }

        public string BulkUploadMessage { get; set; }

        public bool DisplayMigrationButton { get; set; }
    }
}
