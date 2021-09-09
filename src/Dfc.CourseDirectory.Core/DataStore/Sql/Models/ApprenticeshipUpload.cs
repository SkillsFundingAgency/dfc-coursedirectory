using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class ApprenticeshipUpload
    {
        public Guid ApprenticeshipUploadId { get; set; }
        public Guid ProviderId { get; set; }
        public UploadStatus UploadStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ProcessingStartedOn { get; set; }
        public DateTime? ProcessingCompletedOn { get; set; }
        public DateTime? PublishedOn { get; set; }
        public DateTime? AbandonedOn { get; set; }
    }

    public class ApprenticeshipUploadRow
    {
        public int RowNumber { get; set; }
        public bool IsValid { get; set; }
        public IReadOnlyCollection<string> Errors { get; set; }
        public Guid ApprenticeshipId { get; set; }
        public Guid ApprenticeshipLocationId { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime LastValidated { get; set; }
        public int StandardCode{ get; set; }
        public int StandardVersion { get; set; }
        public string ApprenticeshipInformation { get; set; }
        public string ApprenticeshipWebpage { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string ContactUrl { get; set; }
        public string DeliveryMethod { get; set; }
        public string VenueName { get; set; }
        public string YourVenueReference { get; set; }
        public string Radius { get; set; }
        public string DeliveryModes { get; set; }
        public string NationalDelivery { get; set; }
        public string SubRegions { get; set; }
        public Guid? VenueId { get; set; }
    }
}
