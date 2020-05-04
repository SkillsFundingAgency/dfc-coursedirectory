using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models
{
    public class Framework
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        public int FrameworkCode { get; set; }
        public int ProgType { get; set; }
        public int PathwayCode { get; set; }
        public string NasTitle { get; set; }
        public int RecordStatusId { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}
