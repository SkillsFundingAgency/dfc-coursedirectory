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
        public int UKPRN { get; set; }
        [JsonProperty("VENUE_NAME")]
        public string VenueName { get; set; }
        public int Status { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}
