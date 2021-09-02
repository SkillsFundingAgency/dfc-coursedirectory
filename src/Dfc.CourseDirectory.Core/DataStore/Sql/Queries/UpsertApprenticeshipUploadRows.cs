using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertApprenticeshipUploadRows : ISqlQuery<IReadOnlyCollection<ApprenticeshipUploadRow>>
    {
        public Guid ApprenticeshipUploadId { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public DateTime ValidatedOn { get; set; }
        public IEnumerable<UpsertApprenticeshipUploadRowsRecord> Records { get; set; }
    }

    public class UpsertApprenticeshipUploadRowsRecord
    {
        public int RowNumber { get; set; }
        public bool IsValid { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public Guid ApprenticeshipId { get; set; }
        public int StandardCode { get; set; }
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
        public string DeliveryMode { get; set; }
        public string NationalDelivery { get; set; }
        public string SubRegions { get; set; }
        public Guid? VenueId { get; set; }
        public ApprenticeshipDeliveryMode? ResolvedDeliveryMode { get; set; }
        public ApprenticeshipLocationType? ResolvedDeliveryMethod { get; set; }
        public IEnumerable<string> ResolvedSubRegions { get; set; }
        public bool? ResolvedNationalDelivery { get; set; }
        public int? ResolvedRadius { get; set; }
    }
}
