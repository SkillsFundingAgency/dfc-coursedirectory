using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models
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
        public ProviderStatus Status { get; set; }
        public string MarketingInformation { get; set; }
        public string CourseDirectoryName { get; set; }
        public string TradingName { get; set; }
        public string Alias { get; set; }
        public IList<ProviderContact> ProviderContact { get; set; } = new List<ProviderContact>();
        public IList<ProviderAlias> ProviderAliases { get; set; } = new List<ProviderAlias>();
        public DateTime DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class ProviderAlias
    {
        [JsonProperty("ProviderAlias")]
        public string Alias { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class ProviderContact
    {
        public string ContactType { get; set; }
        public ProviderContactPersonalDetails ContactPersonalDetails { get; set; }
        public ProviderContactAddress ContactAddress { get; set; }
        public string ContactTelephone1 { get; set; }
        public string ContactFax { get; set; }
        public string ContactWebsiteAddress { get; set; }
        public string ContactEmail { get; set; }
        public DateTime LastUpdated { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class ProviderContactAddress
    {
        public ProviderContactAddressSAON SAON { get; set; }
        public ProviderContactAddressPAON PAON { get; set; }
        public string StreetDescription { get; set; }
        public string Locality { get; set; }
        public IList<string> Items { get; set; }
        public string PostCode { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class ProviderContactPersonalDetails
    {
        public IList<string> PersonNameTitle { get; set; }
        public IList<string> PersonGivenName { get; set; }
        public string PersonFamilyName { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class ProviderContactAddressSAON
    {
        public string Description { get; set; }
    }

    public class ProviderContactAddressPAON
    {
        public string Description { get; set; }
    }
}
