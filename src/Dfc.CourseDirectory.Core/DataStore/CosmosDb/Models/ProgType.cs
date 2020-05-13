using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models
{
    public class ProgType
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        public int ProgTypeId { get; set; }
        public string ProgTypeDesc { get; set; }
        public string ProgTypeDesc2 { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public DateTime CreatedDateTimeUtc { get; set; }
        public DateTime ModifiedDateTimeUtc { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}
