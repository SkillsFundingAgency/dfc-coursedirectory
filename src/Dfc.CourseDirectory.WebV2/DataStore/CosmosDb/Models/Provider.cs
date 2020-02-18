using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models
{
    public class Provider
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string UnitedKingdomProviderReferenceNumber { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}
