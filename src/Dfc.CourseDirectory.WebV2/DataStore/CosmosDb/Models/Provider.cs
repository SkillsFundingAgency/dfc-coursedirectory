using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.WebV2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models
{
    public class Provider
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        public string UnitedKingdomProviderReferenceNumber { get; set; }
        public string ProviderName { get; set; }
        public ProviderType ProviderType { get; set; }
        public string CourseDirectoryName { get; set; }
        public string Alias { get; set; }
        public string MarketingInformation { get; set; }
        public DateTime DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}
