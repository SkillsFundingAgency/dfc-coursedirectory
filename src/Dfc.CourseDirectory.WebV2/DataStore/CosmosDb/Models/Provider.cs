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
        [JsonIgnore]
        public int Ukprn => int.Parse(UnitedKingdomProviderReferenceNumber);
        public ProviderType ProviderType { get; set; }
        public string ProviderName { get; set; }
        public string ProviderStatus { get; set; }
        public string MarketingInformation { get; set; }
        public string CourseDirectoryName { get; set; }
        public string TradingName { get; set; }
        public string Alias { get; set; }
        public IList<ProviderContact> ProviderContact { get; set; } = new List<ProviderContact>();
        public DateTime DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class ProviderContact
    {
        public string ContactType { get; set; }
        public ContactPersonalDetails ContactPersonalDetails { get; set; }
        public ContactAddress ContactAddress { get; set; }
        public string ContactTelephone1 { get; set; }
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
        public string PostCode { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class ContactPersonalDetails
    {
        public IList<string> PersonNameTitle { get; set; }
        public IList<string> PersonGivenName { get; set; }
        public string PersonFamilyName { get; set; }
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
