using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models
{
    public class SectorSubjectAreaTier1
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        public decimal SectorSubjectAreaTier1Id { get; set; }
        public string SectorSubjectAreaTier1Desc { get; set; }
        public string SectorSubjectAreaTier1Desc2 { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public DateTime CreatedDateTimeUtc { get; set; }
        public DateTime ModifiedDateTimeUtc { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}
