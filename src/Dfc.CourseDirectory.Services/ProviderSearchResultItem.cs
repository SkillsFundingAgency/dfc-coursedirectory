using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services
{
    public class ProviderSearchResultItem : ValueObject<ProviderSearchResultItem>, IProviderSearchResultItem
    {
        public string UnitedKingdomProviderReferenceNumber { get; }
        public string ProviderName { get; }
        public string ProviderStatus { get; }

        public Provider Provider { get; set; }

        public ProviderSearchResultItem(
            string unitedKingdomProviderReferenceNumber,
            string providerName,
            string providerStatus,
            Provider provider)
        {
            Throw.IfNullOrWhiteSpace(unitedKingdomProviderReferenceNumber, nameof(unitedKingdomProviderReferenceNumber));
            Throw.IfNullOrWhiteSpace(providerName, nameof(providerName));
            Throw.IfNullOrWhiteSpace(providerStatus, nameof(providerStatus));
            Throw.IfNull(provider, nameof(provider));

            UnitedKingdomProviderReferenceNumber = unitedKingdomProviderReferenceNumber;
            ProviderName = providerName;
            ProviderStatus = providerStatus;
            Provider = provider;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return UnitedKingdomProviderReferenceNumber;
            yield return ProviderName;
            yield return ProviderStatus;
            yield return Provider;
        }
    }

    public class ProviderContact
    {
        public string ContactType { get; }
        public ContactAddress ContactAddress { get; }
        public ContactPersonalDetails ContactPersonalDetails { get; }
        public string ContactRole { get; }
        public string ContactTelephone1 { get; }
        public string ContactTelephone2 { get; }
        public string ContactFax { get; }
        public string ContactWebsiteAddress { get; }
    }

    public class ContactAddress
    {
        public SAON SAON { get; }
        public PAON PAON { get; }
        public string StreetDescription { get; }
        public string UniqueStreetReferenceNumber { get; }
        public string Locality { get; }
        public string[] Items { get; }
        public string[] ItemsElementName { get; }
        public string PostTown { get; }
        public string PostCode { get; }
        public string UniquePropertyReferenceNumber { get; }
    }

    public class SAON
    {
        public string Description { get; }
    }

    public class PAON
    {
        public string Description { get; }
    }

    public class ContactPersonalDetails
    {
        public string PersonNameTitle { get; }
        public string PersonGivenName { get; }
        public string PersonFamilyName { get; }
        public string PersonNameSuffix { get; }
        public string PersonRequestedName { get; }
    }
}
