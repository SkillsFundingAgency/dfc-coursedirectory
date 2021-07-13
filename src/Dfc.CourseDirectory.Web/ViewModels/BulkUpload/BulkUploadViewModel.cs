using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewModels.BulkUpload
{
    public class BulkUploadViewModel
    {
        public IEnumerable<string> errors { get; set; }
        public string NotificationTitle { get; set; }
        public string NotificationMessage { get; set; }

        // Background process status
        public bool BulkUploadBackgroundInProgress { get; set; }
        public DateTime? BulkUploadBackgroundStartTimestamp { get; set; }
        public int? BulkUploadBackgroundRowCount { get; set; }
    }
}
