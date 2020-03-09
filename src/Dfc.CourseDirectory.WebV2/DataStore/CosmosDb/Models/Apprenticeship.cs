using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models
{
    public class Apprenticeship
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        public int ProviderUKPRN { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public string MarketingInformation { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}
