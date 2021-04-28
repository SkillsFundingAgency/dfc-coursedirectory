using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class VenueUpload
    {
        public Guid VenueUploadId { get; set; }
        public Guid ProviderId { get; set; }
        public UploadStatus UploadStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ProcessingStartedOn { get; set; }
        public DateTime? ProcessingCompletedOn { get; set; }
        public DateTime? PublishedOn { get; set; }
        public DateTime? AbandonedOn { get; set; }
        public DateTime? LastValidated { get; set; }
    }

    public class VenueUploadRow : VenueRow
    {
        public int RowNumber { get; set; }
        public bool IsValid { get; set; }
        public IReadOnlyCollection<string> Errors { get; set; }
        public bool IsSupplementary { get; set; }
        public bool? OutsideOfEngland { get; set; }
        public Guid? VenueId { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime LastValidated { get; set; }
    }
}
