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
        private const ProviderType NewProviderProviderType = ProviderType.None;
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
            _logger.LogInformation($"UKRLP Sync: Beginning {nameof(SyncAllProviderData)}, fetching providers from UKRLP API...");
            var allProviders = await _ukrlpService.GetAllProviderData(updatedSince);
            _logger.LogInformation($"UKRLP Sync: {allProviders.Count} providers received, processing...");

            var createdCount = 0;
            var updatedCount = 0;

            foreach (var providerData in allProviders)
            {
                _logger.LogDebug($"UKRLP Sync: processing provider {createdCount+updatedCount+1} of {allProviders.Count}, UKPRN: {providerData.UnitedKingdomProviderReferenceNumber}...");
                var result = await CreateOrUpdateProvider(providerData);

                if (result == CreateOrUpdateResult.Created)
                {
                    createdCount++;
                }
                else  // result == CreateOrUpdateResult.Updated
                {
                    updatedCount++;
                }

                if ((createdCount + updatedCount) % 200 == 0)
                {
                    _logger.LogInformation(
                        $"UKRLP Sync: processed provider {createdCount + updatedCount} of {allProviders.Count}, UKPRN: {providerData.UnitedKingdomProviderReferenceNumber}...");
                }
            }

            _logger.LogInformation("UKRLP Sync: Added {0} new providers and updated {1} providers.", createdCount, updatedCount);
        }

        public async Task SyncProviderData(int ukprn)
        {
            _logger.LogDebug($"UKRLP Sync: Fetching updated UKRLP data for UKPRN {ukprn}...");
            var providerData = await _ukrlpService.GetProviderData(ukprn);

            if (providerData == null)
            {
                _logger.LogWarning("UKRLP Sync: Failed to update provider information from UKRLP for {0}.", ukprn);

                return;
            }

            await CreateOrUpdateProvider(providerData);

            _logger.LogInformation("UKRLP Sync: Successfully updated provider information from UKRLP for {0}.", ukprn);
        }

        // internal for testing
        internal static ProviderContactStructure SelectContact(IEnumerable<ProviderContactStructure> contacts) =>
            contacts
                .Where(c => c.ContactType == "P")
                .OrderByDescending(c => c.LastUpdated)
                .FirstOrDefault();

        private static ProviderAlias MapAlias(ProviderAliasesStructure alias) => new ProviderAlias()
        {
            Alias = alias.ProviderAlias
        };

        private static ProviderContact MapContact(ProviderContactStructure contact) => new ProviderContact()
        {
            ContactAddress = new ProviderContactAddress()
            {
                SAON = new ProviderContactAddressSAON { Description = contact.ContactAddress.Address1 },
                PAON = new ProviderContactAddressPAON { Description = contact.ContactAddress.Address2 },
                StreetDescription = contact.ContactAddress.Address3,
                Locality = contact.ContactAddress.Address4,
                Items = new List<string>
                {
                    contact.ContactAddress.Town,
                    contact.ContactAddress.County,
                }.Where(s => s != null).ToList(),
                PostTown = contact.ContactAddress.Town,
                County = contact.ContactAddress.County,
                PostCode = contact.ContactAddress.PostCode,
            },
            ContactEmail = contact.ContactEmail,
            ContactFax = contact.ContactFax,
            ContactPersonalDetails = new ProviderContactPersonalDetails()
            {
                PersonNameTitle = contact.ContactPersonalDetails.PersonNameTitle,
                PersonGivenName = contact.ContactPersonalDetails.PersonGivenName,
                PersonFamilyName = contact.ContactPersonalDetails.PersonFamilyName,
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
                        Aliases = providerData.ProviderAliases.Select(MapAlias),
                        DateUpdated = _clock.UtcNow,
                        ProviderId = providerId,
                        Contacts = contact != null ?
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
                        Aliases = providerData.ProviderAliases.Select(MapAlias),
                        DateUpdated = _clock.UtcNow,
                        ProviderId = existingProvider.Id,
                        Contacts = contact != null ?
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
