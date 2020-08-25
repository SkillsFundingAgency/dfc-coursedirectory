using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models
{
    public class Apprenticeship
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        public Guid ProviderId { get; set; }
        public int ProviderUKPRN { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public ApprenticeshipType ApprenticeshipType { get; set; }
        public Guid? FrameworkId { get; set; }
        public Guid? StandardId { get; set; }
        public int? FrameworkCode { get; set; }
        public int? ProgType { get; set; }
        public int? PathwayCode { get; set; }
        public int? StandardCode { get; set; }
        public int? Version { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string MarketingInformation { get; set; }
        public string Url { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactWebsite { get; set; }
        public List<ApprenticeshipLocation> ApprenticeshipLocations { get; set; }
        public int RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public int? ApprenticeshipId { get; set; }
        public List<BulkUploadError> BulkUploadErrors { get; set; }
        public IEnumerable<string> ValidationErrors { get; set; }
        public IEnumerable<string> LocationValidationErrors { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class ApprenticeshipLocation
    {
        public Guid Id { get; set; }
        public Guid? VenueId { get; set; }
        public bool? National { get; set; }
        public ApprenticeshipLocationAddress Address { get; set; }
        public List<ApprenticeshipDeliveryMode> DeliveryModes { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public int ProviderUKPRN { get; set; }
        public IEnumerable<string> Regions { get; set; }
        public ApprenticeshipLocationType ApprenticeshipLocationType { get; set; }
        public LocationType LocationType { get; set; }
        public int? Radius { get; set; }
        public int RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        // ApprenticeshipLocationId should really be Nullable<int> but that's not compatible with the model used elsewhere
        public int ApprenticeshipLocationId { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class ApprenticeshipLocationAddress
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string County { get; set; }
        public string Email { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Phone { get; set; }
        public string Postcode { get; set; }
        public string Town { get; set; }
        public string Website { get; set; }
    }
}
