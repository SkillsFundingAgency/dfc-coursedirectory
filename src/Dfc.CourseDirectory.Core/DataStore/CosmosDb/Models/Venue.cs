using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dfc.CourseDirectory.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class Venue
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("UKPRN")]
        public int Ukprn { get; set; }

        [JsonProperty("VENUE_NAME")]
        public string VenueName { get; set; }

        [JsonProperty("ADDRESS_1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("ADDRESS_2")]
        public string AddressLine2 { get; set; }

        [JsonProperty("TOWN")]
        public string Town { get; set; }

        [JsonProperty("COUNTY")]
        public string County { get; set; }

        [JsonProperty("POSTCODE")]
        public string Postcode { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public string PHONE { get; set; }

        [JsonProperty("EMAIL")]
        public string Email { get; set; }

        [JsonProperty("WEBSITE")]
        public string Website { get; set; }

        public VenueStatus Status { get; set; }

        public int? LocationId { get; set; }

        [JsonProperty("PROV_VENUE_ID")]
        public string ProvVenueID { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime DateUpdated { get; set; }

        public string UpdatedBy { get; set; }

        [Obsolete("Use PHONE instead.")]
        public string Telephone { get => PHONE; set => PHONE = value; }

        // Website - obsolete, use WEBSITE (CosmosDB store is case sensitive, both exist)
        //
        // Email - obsolete, use EMAIL (CosmosDB store is case sensitive, both exist)

        public bool ShouldSerializeTelephone() => false;

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }

        private string DebuggerDisplay => $"Venue: '{VenueName}' Status: {Status} Id: {Id}";
    }
}
