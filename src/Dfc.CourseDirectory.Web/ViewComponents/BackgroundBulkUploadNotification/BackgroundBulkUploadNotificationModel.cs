using System;

namespace Dfc.CourseDirectory.Web.ViewComponents.BackgroundBulkUploadNotification
{
    public class BackgroundBulkUploadNotificationModel
    {
        // Correspond with the new model changes on provider to track status of background bulk upload process
        public bool BulkUploadBackgroundInProgress { get; set; }
        public DateTime? BulkUploadBackgroundStartTimestamp { get; set; }
        public int? BulkUploadBackgroundRowCount { get; set; }
        public bool BulkUploadPublishInProgress { get; set; }
    }
}
