using System;

namespace Dfc.CourseDirectory.Models.Models.Providers
{
    public class BulkUploadStatus
    {
        public bool InProgress { get; set; }
        public DateTime? StartedTimestamp { get; set; }
        public int? TotalRowCount { get; set; }
        public bool PublishInProgress { get; set; }
    }
}
