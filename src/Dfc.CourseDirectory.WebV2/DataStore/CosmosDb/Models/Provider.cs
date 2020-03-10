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
        public ProviderType ProviderType { get; set; }
        public string ProviderName { get; set; }
        public string ProviderStatus { get; set; }
        public IList<ProviderContact> ProviderContact { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class ProviderContact
    {
        public string ContactType { get; set; }
        public ContactAddress ContactAddress { get; set; }
        public object ContactRole { get; set; }
        public string ContactTelephone1 { get; set; }
        public object ContactTelephone2 { get; set; }
        public string ContactFax { get; set; }
        public string ContactWebsiteAddress { get; set; }
        public string ContactEmail { get; set; }
        public DateTime LastUpdated { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class ContactAddress
    {
        public SAON SAON { get; set; }
        public PAON PAON { get; set; }
        public string StreetDescription { get; set; }
        public string Locality { get; set; }
        public IList<string> Items { get; set; }
        public IList<int> ItemsElementName { get; set; }
        public string PostCode { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class SAON
    {
        public string Description { get; set; }
    }

    public class PAON
    {
        public string Description { get; set; }
    }
}
