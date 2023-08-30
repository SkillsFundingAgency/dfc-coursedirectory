using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
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
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IClock _clock;
        private readonly ILogger<UkrlpSyncHelper> _logger;

        public UkrlpSyncHelper(
            IUkrlpService ukrlpService,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IClock clock,
            ILoggerFactory loggerFactory)
        {
            _ukrlpService = ukrlpService;
            _sqlQueryDispatcher = sqlQueryDispatcher;
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
                _logger.LogDebug($"UKRLP Sync: processing provider {createdCount + updatedCount + 1} of {allProviders.Count}, UKPRN: {providerData.UnitedKingdomProviderReferenceNumber}...");
                var result = await CreateOrUpdateProvider(providerData);

                if (result == CreateOrUpdateResult.Created)
                {
                    createdCount++;
                }
                else  
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

        public Task SyncAllKnownProvidersData()
        {
            const int chunkSize = 200;

            return _sqlQueryDispatcher.ExecuteQuery(new ProcessAllProviders()
            {
                ProcessChunk = async providers =>
                {
                    foreach (var chunk in providers.Buffer(chunkSize))
                    {
                        await SyncProviderData(chunk.Select(p => p.Ukprn));
                    }
                }
            });
        }

        public async Task SyncProviderData(int ukprn)
        {
            _logger.LogDebug($"UKRLP Sync: Fetching updated UKRLP data for UKPRN {ukprn}...");
            (await _ukrlpService.GetProviderData(new[] { ukprn })).TryGetValue(ukprn, out var providerData);

            if (providerData == null)
            {
                _logger.LogWarning("UKRLP Sync: Failed to update provider information from UKRLP for {0}.", ukprn);

                return;
            }

            await CreateOrUpdateProvider(providerData);

            _logger.LogInformation("UKRLP Sync: Successfully updated provider information from UKRLP for {0}.", ukprn);
        }

        public async Task SyncProviderData(IEnumerable<int> ukprns)
        {
            _logger.LogDebug($"UKRLP Sync: Fetching updated UKRLP data for UKPRNs {string.Join(", ", ukprns)}...");

            var allProviderData = await _ukrlpService.GetProviderData(ukprns);

            foreach (var ukprn in ukprns)
            {
                if (allProviderData.TryGetValue(ukprn, out var providerData))
                {
                    await CreateOrUpdateProvider(providerData);

                    _logger.LogInformation("UKRLP Sync: Successfully updated provider information from UKRLP for {0}.", ukprn);
                }
                else
                {
                    _logger.LogWarning("UKRLP Sync: Failed to update provider information from UKRLP for {0}.", ukprn);
                }
            }
        }

        // internal for testing
        internal static ProviderContactStructure SelectContact(IEnumerable<ProviderContactStructure> contacts) =>
            contacts
                .Where(c => c.ContactType == "P")
                .OrderByDescending(c => c.LastUpdated)
                .FirstOrDefault();

        //private static ProviderAlias MapAlias(ProviderAliasesStructure alias) => new ProviderAlias()
        //{
        //    Alias = alias.ProviderAlias
        //};

        private static ProviderContact MapContact(ProviderContactStructure contact) => new ProviderContact()
        {
            AddressSaonDescription = contact.ContactAddress.Address1,
            AddressPaonDescription = contact.ContactAddress.Address2,
            AddressStreetDescription = contact.ContactAddress.Address3,
            AddressLocality = contact.ContactAddress.Address4,
            AddressItems = contact.ContactAddress.Town + " " + contact.ContactAddress.County,
            AddressPostTown = contact.ContactAddress.Town,
            AddressCounty = contact.ContactAddress.County,
            AddressPostcode = contact.ContactAddress.PostCode,
            Email = contact.ContactEmail,
            Fax = contact.ContactFax,
            PersonalDetailsPersonNameTitle = contact.ContactPersonalDetails.PersonNameTitle[0],
            PersonalDetailsPersonNameGivenName = contact.ContactPersonalDetails.PersonGivenName[0],
            PersonalDetailsPersonNameFamilyName = contact.ContactPersonalDetails.PersonFamilyName,
            Telephone1 = contact.ContactTelephone1,
            ContactType = contact.ContactType,
            WebsiteAddress = contact.ContactWebsiteAddress,
            //LastUpdated = contact.LastUpdated
        };

        private async Task<CreateOrUpdateResult> CreateOrUpdateProvider(ProviderRecordStructure providerData)
        {
            var ukprn = int.Parse(providerData.UnitedKingdomProviderReferenceNumber);

            var existingProvider = await GetProvider(ukprn);

            var contact = SelectContact(providerData.ProviderContact);

            var providerId = existingProvider?.ProviderId ?? Guid.NewGuid();

            if (existingProvider == null)
            {
                await _sqlQueryDispatcher.ExecuteQuery(
                    new CreateProviderFromUkrlpData()
                    {
                        Alias = providerData.ProviderAliases.FirstOrDefault()?.ProviderAlias,
                        //Aliases = providerData.ProviderAliases.Select(MapAlias),
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
                await _sqlQueryDispatcher.ExecuteQuery(
                    new UpdateProviderFromUkrlpData()
                    {
                        Alias = providerData.ProviderAliases.FirstOrDefault()?.ProviderAlias,
                        //Aliases = providerData.ProviderAliases.Select(MapAlias),
                        DateUpdated = _clock.UtcNow,
                        ProviderId = providerId,
                        Contacts = contact != null ?
                            new List<ProviderContact>() { MapContact(contact) } :
                            new List<ProviderContact>(),
                        ProviderName = providerData.ProviderName,
                        ProviderStatus = providerData.ProviderStatus,
                        UpdatedBy = UpdatedBy
                    });
                _logger.LogInformation("UKRLP Sync: Update {0} starting...", ukprn);

                var oldStatusCode = MapProviderStatusDescription(existingProvider.ProviderStatus);
                var newStatusCode = MapProviderStatusDescription(providerData.ProviderStatus);

                var deactivating = IsDeactivatedStatus(newStatusCode);
                _logger.LogInformation("UKRLP Sync: ukprn {0} - oldStatusCode {1} - newStatusCode {2} - deactivating {3}", ukprn, oldStatusCode, newStatusCode, deactivating);
                if (deactivating)
                {
                    _logger.LogInformation("UKRLP Sync: Update {0} starting deactivating is {1} ...", ukprn, deactivating);
                   
                        await _sqlQueryDispatcher.ExecuteQuery(new DeleteCoursesForProvider() { ProviderId = providerId });
                        await _sqlQueryDispatcher.ExecuteQuery(new DeleteTLevelsForProvider() { ProviderId = providerId });
                        await _sqlQueryDispatcher.ExecuteQuery(new MarkFindACourseIndexNotLiveForDeactiveProvider() { ProviderId = providerId });
                        await _sqlQueryDispatcher.Commit();
                    
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
           _sqlQueryDispatcher.ExecuteQuery(new GetProviderByUkprn() { Ukprn = ukprn });

        private enum CreateOrUpdateResult { Created, Updated }
    }
}
