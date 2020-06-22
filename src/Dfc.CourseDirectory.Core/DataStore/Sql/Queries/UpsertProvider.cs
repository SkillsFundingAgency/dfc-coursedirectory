using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertProvider : ISqlQuery<None>
    {
        public Guid ProviderId { get; set; }
        public int Ukprn { get; set; }
        public ProviderStatus ProviderStatus { get; set; }
        public ProviderType ProviderType { get; set; }
        public string ProviderName { get; set; }
        public string UkrlpProviderStatusDescription { get; set; }
        public string MarketingInformation { get; set; }
        public string CourseDirectoryName { get; set; }
        public string TradingName { get; set; }
        public string Alias { get; set; }
        public IEnumerable<UpsertProviderContact> Contacts { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class UpsertProviderContact
    {
        public string ContactType { get; set; }
        public string ContactRole { get; set; }
        public string AddressSaonDescription { get; set; }
        public string AddressPaonDescription { get; set; }
        public string AddressStreetDescription { get; set; }
        public string AddressLocality { get; set; }
        public string AddressItems { get; set; }
        public string AddressPostTown { get; set; }
        public string AddressPostcode { get; set; }
        public string PersonalDetailsPersonNameTitle { get; set; }
        public string PersonalDetailsPersonNameGivenName { get; set; }
        public string PersonalDetailsPersonNameFamilyName { get; set; }
    }
}
