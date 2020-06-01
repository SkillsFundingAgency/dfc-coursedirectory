using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models
{
    public class Standard
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        public int StandardCode { get; set; }
        public int Version { get; set; }
        public string StandardName { get; set; }
        public string StandardSectorCode { get; set; }
        public string NotionalEndLevel { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string URLLink { get; set; }
        public decimal? SectorSubjectAreaTier1 { get; set; }
        public decimal? SectorSubjectAreaTier2 { get; set; }
        public string OtherBodyApprovalRequired { get; set; }
        public int RecordStatusId { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}
