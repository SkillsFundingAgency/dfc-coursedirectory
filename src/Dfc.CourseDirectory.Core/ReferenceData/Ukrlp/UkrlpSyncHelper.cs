using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.Extensions.Logging;
using UkrlpService;

namespace Dfc.CourseDirectory.Core.ReferenceData.Ukrlp
{
    public class UkrlpSyncHelper
    {
        private const ProviderType NewProviderProviderType = ProviderType.Both;
        private const ProviderStatus NewProviderProviderStatus = ProviderStatus.Registered;
        private const string UpdatedBy = nameof(UkrlpSyncHelper);

        private readonly IUkrlpService _ukrlpService;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IClock _clock;
        private readonly ILogger<UkrlpSyncHelper> _logger;

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

        public async Task SyncAllProviderData(DateTime updatedSince)
        {
            var allProviders = await _ukrlpService.GetAllProviderData(updatedSince);

            var createdCount = 0;
            var updatedCount = 0;

            foreach (var providerData in allProviders)
            {
                var result = await CreateOrUpdateProvider(providerData);

                if (result == CreateOrUpdateResult.Created)
                {
                    createdCount++;
                }
                else  // result == CreateOrUpdateResult.Updated
                {
                    updatedCount++;
                }
            }

            _logger.LogInformation("Added {0} new providers and updated {1} providers.", createdCount, updatedCount);
        }

        public async Task SyncProviderData(int ukprn)
        {
            var providerData = await _ukrlpService.GetProviderData(ukprn);

            if (providerData == null)
            {
                _logger.LogWarning("Failed to update provider information from UKRLP for {0}.", ukprn);

                return;
            }

            await CreateOrUpdateProvider(providerData);

            _logger.LogInformation("Successfully updated provider information from UKRLP for {0}.", ukprn);
        }

        // internal for testing
        internal static ProviderContactStructure SelectContact(IEnumerable<ProviderContactStructure> contacts) =>
            contacts
                .Where(c => c.ContactType == "P")
                .OrderByDescending(c => c.LastUpdated)
                .FirstOrDefault();

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

        private async Task<CreateOrUpdateResult> CreateOrUpdateProvider(ProviderRecordStructure providerData)
        {
            var ukprn = int.Parse(providerData.UnitedKingdomProviderReferenceNumber);

            var existingProvider = await GetProvider(ukprn);

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
                        ProviderStatus = providerData.ProviderStatus,
                        ProviderType = NewProviderProviderType,
                        Status = NewProviderProviderStatus,
                        Ukprn = ukprn,
                        UpdatedBy = UpdatedBy
                    });

                return CreateOrUpdateResult.Created;
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
                        ProviderStatus = providerData.ProviderStatus,
                        UpdatedBy = UpdatedBy
                    });

                var oldStatusCode = MapProviderStatusDescription(existingProvider.ProviderStatus);
                var newStatusCode = MapProviderStatusDescription(providerData.ProviderStatus);

                var deactivating = !IsDeactivatedStatus(oldStatusCode) && IsDeactivatedStatus(newStatusCode);

                if (deactivating)
                {
                    await _cosmosDbQueryDispatcher.ExecuteQuery(new ArchiveApprenticeshipsForProvider() { Ukprn = ukprn });
                    await _cosmosDbQueryDispatcher.ExecuteQuery(new ArchiveCoursesForProvider() { Ukprn = ukprn });
                }

                return CreateOrUpdateResult.Updated;
            }
        }

        private static bool IsDeactivatedStatus(string status) => status switch
        {
            "A" => false,
            "PD1" => true,
            "PD2" => true,
            _ => throw new NotImplementedException($"Unknown status: '{status}'.")
        };

        private static string MapProviderStatusDescription(string statusDesc) => statusDesc switch
        {
            "Active" => "A",
            "Provider deactivated, not verified" => "PD1",
            "Provider deactivated, verified" => "PD2",
            _ => throw new NotImplementedException($"Unknown status: '{statusDesc}'.")
        };

        private Task<Provider> GetProvider(int ukprn) =>
           _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderByUkprn() { Ukprn = ukprn });

        private enum CreateOrUpdateResult { Created, Updated }
    }
}
