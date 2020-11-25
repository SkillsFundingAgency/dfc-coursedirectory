using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.Providerportal.FindAnApprenticeship.Interfaces.Services;
using Dfc.Providerportal.FindAnApprenticeship.Models.Providers;

namespace Dfc.Providerportal.FindAnApprenticeship.Helper
{
    public class ProviderService : IProviderService
    {
        private readonly Func<IDbConnection> _dbConnectionFactory;

        public ProviderService(Func<IDbConnection> dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public async Task<IEnumerable<Provider>> GetActiveProvidersAsync()
        {
            const string sql = @"
select * from Pttcd.ProviderContacts pc
where pc.ProviderId IN (
    select p.ProviderId
    from Pttcd.Providers p
    where p.ProviderStatus = 1 and p.UkrlpProviderStatusDescription = 'Active');

select * from Pttcd.Providers p
where p.ProviderStatus = 1 and p.UkrlpProviderStatusDescription = 'Active'";

            using (var con = _dbConnectionFactory())
            {
                con.Open();

                using (var multi = await con.QueryMultipleAsync(sql))
                {
                    var providerContacts = (await multi.ReadAsync<ProviderContactDataRecord>())
                        .ToLookup(c => c.ProviderId, c => new Providercontact
                        {
                            ContactType = c.ContactType.ToString(),
                            ContactAddress = new Contactaddress
                            {
                                SAON = new SAON
                                {
                                    Description = c.AddressSaonDescription
                                },
                                PAON = new PAON
                                {
                                    Description = c.AddressPaonDescription
                                },
                                StreetDescription = c.AddressStreetDescription,
                                UniqueStreetReferenceNumber = null,
                                Locality = c.AddressLocality,
                                Items = new[] { c.AddressItems },
                                ItemsElementName = null,
                                PostTown = c.AddressPostTown,
                                PostCode = c.AddressPostcode,
                                UniquePropertyReferenceNumber = null
                            },
                            ContactPersonalDetails = new Contactpersonaldetails
                            {
                                PersonNameTitle = new[] { c.PersonalDetailsPersonNameTitle },
                                PersonGivenName = new[] { c.PersonalDetailsPersonNameGivenName },
                                PersonFamilyName = c.PersonalDetailsPersonNameFamilyName,
                                PersonNameSuffix = null,
                                PersonRequestedName = null
                            },
                            ContactRole = c.ContactRole,
                            ContactTelephone1 = c.Telephone1,
                            ContactTelephone2 = c.Telephone2,
                            ContactFax = c.Fax,
                            ContactWebsiteAddress = c.WebsiteAddress,
                            ContactEmail = c.Email,
                            LastUpdated = null
                        });

                    return (await multi.ReadAsync<ProviderDataRecord>())
                        .Select(p => new Provider
                        {
                            Id = p.ProviderId,
                            UnitedKingdomProviderReferenceNumber = p.Ukprn.ToString(),
                            ProviderName = p.ProviderName,
                            CourseDirectoryName = p.CourseDirectoryName,
                            ProviderStatus = p.UkrlpProviderStatusDescription,
                            ProviderContact = providerContacts[p.ProviderId].ToArray(),
                            ProviderVerificationDate = null,
                            ProviderVerificationDateSpecified = null,
                            ExpiryDateSpecified =  null,
                            ProviderAssociations =  null,
                            ProviderAliases = null,
                            VerificationDetails = null,
                            Status = (Status?)p.ProviderStatus ?? default,
                            ProviderId = p.TribalProviderId,
                            UPIN = null,
                            TradingName = p.TradingName,
                            NationalApprenticeshipProvider = p.NationalApprenticeshipProvider,
                            MarketingInformation = p.MarketingInformation,
                            Alias = p.Alias,
                            ProviderType = (ProviderType?)p.ProviderType ?? default,
                            DisplayNameSource = (ProviderDisplayNameSource)p.DisplayNameSource
                        })
                        .ToArray();
                }
            }
        }

        private class ProviderDataRecord
        {
            public Guid ProviderId { get; set; }
            public int? ApprenticeshipQAStatus { get; set; }
            public int? Ukprn { get; set; }
            public int? ProviderStatus { get; set; }
            public int? ProviderType { get; set; }
            public string ProviderName { get; set; }
            public string UkrlpProviderStatusDescription { get; set; }
            public string MarketingInformation { get; set; }
            public string CourseDirectoryName { get; set; }
            public string TradingName { get; set; }
            public string Alias { get; set; }
            public DateTime? UpdatedOn { get; set; }
            public string UpdatedBy { get; set; }
            public int DisplayNameSource { get; set; }
            public bool NationalApprenticeshipProvider { get; set; }
            public int? TribalProviderId { get; set; }
        }

        private class ProviderContactDataRecord
        {
            public long ProviderContactId { get; set; }
            public Guid ProviderId { get; set; }
            public int ProviderContactIndex { get; set; }
            public char? ContactType { get; set; }
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
            public string Telephone1 { get; set; }
            public string Telephone2 { get; set; }
            public string Fax { get; set; }
            public string WebsiteAddress { get; set; }
            public string Email { get; set; }
        }
    }
}