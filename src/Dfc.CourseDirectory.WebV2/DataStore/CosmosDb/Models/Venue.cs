using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models
{
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
        [JsonProperty("PHONE")]
        public string Telephone { get; set; }
        [JsonProperty("EMAIL")]
        public string Email { get; set; }
        [JsonProperty("WEBSITE")]
        public string Website { get; set; }
        public int Status { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}
