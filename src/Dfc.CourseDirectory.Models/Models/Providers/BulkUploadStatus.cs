using System;

namespace Dfc.CourseDirectory.Models.Models.Providers
{
    public class BulkUploadStatus
    {
        public bool InProgress { get; set; } // bulk upload in p
        public DateTime? StartedTimestamp { get; set; }
        public int? TotalRowCount { get; set; }
        public bool PublishInProgress { get; set; }
    }
}
