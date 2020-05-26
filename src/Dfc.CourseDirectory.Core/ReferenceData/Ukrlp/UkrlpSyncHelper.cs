using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using Microsoft.Extensions.Logging;
using UkrlpService;

namespace Dfc.CourseDirectory.WebV2.Helpers
{
    public class UkrlpSyncHelper
    {
        private const ProviderType NewProviderProviderType = ProviderType.Both;
        private const ProviderStatus NewProviderProviderStatus = ProviderStatus.Registered;
        private const string UpdatedBy = nameof(UkrlpSyncHelper);

        private readonly IUkrlpService _ukrlpService;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IClock _clock;
        private ILogger<UkrlpSyncHelper> _logger;

        public UkrlpSyncHelper(
            IUkrlpService ukrlpService, 
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher, 
            IClock clock,
            ILoggerFactory loggerFactory)
        {
            _ukrlpService = ukrlpService;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _clock = clock;
            _logger = loggerFactory.CreateLogger<UkrlpSyncHelper>();
        }

        public async Task SyncProviderData(int ukprn)
        {
            var providerData = await _ukrlpService.GetProviderData(ukprn);

            if (providerData == null)
            {
                _logger.LogWarning("Failed to update provider information from UKRLP for {0}.", ukprn);

                return;
            }

            var existingProvider = await GetProvider(ukprn);

            var providerStatusDesc = GetProviderStatusDescription(providerData.ProviderStatus);

            var contact = SelectContact(providerData.ProviderContact);

            if (existingProvider == null)
            {
                var providerId = Guid.NewGuid();

                await _cosmosDbQueryDispatcher.ExecuteQuery(
                    new CreateProviderFromUkrlpData()
                    {
                        Alias = providerData.ProviderAliases.FirstOrDefault()?.ProviderAlias,
                        DateUpdated = _clock.UtcNow,
                        ProviderId = providerId,
                        ProviderContact = contact != null ?
                            new List<ProviderContact>() { MapContact(contact) } :
                            new List<ProviderContact>(),
                        ProviderName = providerData.ProviderName,
                        ProviderStatus = providerStatusDesc,
                        ProviderType = NewProviderProviderType,
                        Status = NewProviderProviderStatus,
                        Ukprn = ukprn,
                        UpdatedBy = UpdatedBy
                    });
            }
            else
            {
                await _cosmosDbQueryDispatcher.ExecuteQuery(
                    new UpdateProviderFromUkrlpData()
                    {
                        Alias = providerData.ProviderAliases.FirstOrDefault()?.ProviderAlias,
                        DateUpdated = _clock.UtcNow,
                        ProviderId = existingProvider.Id,
                        ProviderContact = contact != null ?
                            new List<ProviderContact>() { MapContact(contact) } :
                            new List<ProviderContact>(),
                        ProviderName = providerData.ProviderName,
                        ProviderStatus = providerStatusDesc,
                        UpdatedBy = UpdatedBy
                    });
            }

            _logger.LogInformation("Successfully updated provider information from UKRLP for {0}.", ukprn);
        }

        // internal for testing
        internal static ProviderContactStructure SelectContact(IEnumerable<ProviderContactStructure> contacts) =>
            contacts
                .Where(c => c.ContactType == "P")
                .OrderByDescending(c => c.LastUpdated)
                .FirstOrDefault();

        // See https://skillsfundingagency.atlassian.net/wiki/spaces/DFC/pages/873136299/CD+Beta+-+UKRLP
        private static string GetProviderStatusDescription(string ukrlpProviderStatus) =>
            ukrlpProviderStatus switch
            {
                "A" => "Active",
                "V" => "Verified",
                "PD1" => "Deactivation in process",
                "PD2" => "Deactivation complete",
                _ => throw new NotSupportedException()
            };

        private static ProviderContact MapContact(ProviderContactStructure contact) => new ProviderContact()
        {
            ContactAddress = new ContactAddress()
            {
                SAON = new SAON() { Description = contact.ContactAddress.SAON.Description },
                PAON = new PAON() { Description = contact.ContactAddress.PAON.Description },
                StreetDescription = contact.ContactAddress.StreetDescription,
                Locality = contact.ContactAddress.Locality,
                Items = contact.ContactAddress.Items,
                PostCode = contact.ContactAddress.PostCode
            },
            ContactEmail = contact.ContactEmail,
            ContactFax = contact.ContactFax,
            ContactPersonalDetails = new ContactPersonalDetails()
            {
                PersonNameTitle = contact.ContactPersonalDetails.PersonNameTitle,
                PersonGivenName = contact.ContactPersonalDetails.PersonGivenName,
                PersonFamilyName = contact.ContactPersonalDetails.PersonFamilyName
            },
            ContactTelephone1 = contact.ContactTelephone1,
            ContactType = contact.ContactType,
            ContactWebsiteAddress = contact.ContactWebsiteAddress,
            LastUpdated = contact.LastUpdated
        };

        private Task<Provider> GetProvider(int ukprn) =>
           _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderByUkprn() { Ukprn = ukprn });
    }
}
