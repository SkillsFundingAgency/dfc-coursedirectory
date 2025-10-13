using System;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class ProviderUpload
    {
        public Guid ProviderUploadId { get; set; }
        public UploadStatus UploadStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ProcessingStartedOn { get; set; }
        public DateTime? ProcessingCompletedOn { get; set; }
        public DateTime? PublishedOn { get; set; }
        public DateTime? AbandonedOn { get; set; }
    }
}
