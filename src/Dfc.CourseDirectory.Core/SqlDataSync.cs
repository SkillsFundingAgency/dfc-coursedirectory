using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Microsoft.Extensions.Logging;
using Polly;

namespace Dfc.CourseDirectory.Core
{
    public class SqlDataSync
    {
        private const int BatchSize = 150;

        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IClock _clock;
        private readonly ILogger _logger;

        public SqlDataSync(
            ISqlQueryDispatcherFactory sqlQueryDispatcherFactory,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            IClock clock,
            ILogger<SqlDataSync> logger)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _clock = clock;
            _logger = logger;
        }

        public async Task SyncAll()
        {
            await SyncAllProviders();
        }

        public Task SyncAllProviders() => WithExclusiveSqlLock(
            nameof(SyncAllProviders),
            () => _cosmosDbQueryDispatcher.ExecuteQuery(
                new ProcessAllProviders()
                {
                    ProcessChunk = GetSyncWithBatchingHandler<Provider>(SyncProviders)
                }));

        public Task SyncProvider(Provider provider) => SyncProviders(new[] { provider });

        public Task SyncProviders(IEnumerable<Provider> providers) => WithSqlDispatcher(dispatcher =>
            dispatcher.ExecuteQuery(new UpsertProvidersFromCosmos()
            {
                Records = providers.Select(provider => new UpsertProvidersRecord()
                {
                    ProviderId = provider.Id,
                    Ukprn = provider.Ukprn,
                    ProviderStatus = provider.Status,
                    ProviderType = provider.ProviderType,
                    ProviderName = provider.ProviderName,
                    UkrlpProviderStatusDescription = provider.ProviderStatus,
                    MarketingInformation = provider.MarketingInformation,
                    CourseDirectoryName = provider.CourseDirectoryName,
                    TradingName = provider.TradingName,
                    Alias = provider.Alias,
                    UpdatedOn = provider.DateUpdated != default ? (DateTime?)provider.DateUpdated : null,
                    UpdatedBy = provider.UpdatedBy,
                    NationalApprenticeshipProvider = provider.NationalApprenticeshipProvider,
                    TribalProviderId = provider.ProviderId,
                    BulkUploadInProgress = provider.BulkUploadStatus?.InProgress,
                    BulkUploadPublishInProgress = provider.BulkUploadStatus?.PublishInProgress,
                    BulkUploadStartedDateTime = provider.BulkUploadStatus?.StartedTimestamp,
                    BulkUploadTotalRowCount = provider.BulkUploadStatus?.TotalRowCount,
                    Contacts = (provider.ProviderContact ?? Array.Empty<ProviderContact>()).Select(c => new UpsertProvidersRecordContact()
                    {
                        ContactType = c.ContactType,
                        ContactRole = c.ContactRole,
                        AddressSaonDescription = c.ContactAddress?.SAON?.Description,
                        AddressPaonDescription = c.ContactAddress?.PAON?.Description,
                        AddressStreetDescription = c.ContactAddress?.StreetDescription,
                        AddressLocality = c.ContactAddress?.Locality,
                        AddressItems = string.Join(" ", c.ContactAddress?.Items ?? Array.Empty<string>()),
                        AddressPostTown = c.ContactAddress?.PostTown ?? c.ContactAddress?.Items?.ElementAtOrDefault(0),
                        AddressCounty = c.ContactAddress?.County ?? c.ContactAddress?.Items?.ElementAtOrDefault(1),
                        AddressPostcode = c.ContactAddress?.PostCode,
                        PersonalDetailsPersonNameTitle = string.Join(" ", c.ContactPersonalDetails?.PersonNameTitle ?? Array.Empty<string>()),
                        PersonalDetailsPersonNameGivenName = string.Join(" ", c.ContactPersonalDetails?.PersonGivenName ?? Array.Empty<string>()),
                        PersonalDetailsPersonNameFamilyName = c.ContactPersonalDetails?.PersonFamilyName,
                        Telephone1 = c.ContactTelephone1,
                        Telephone2 = c.ContactTelephone2,
                        Fax = c.ContactFax,
                        WebsiteAddress = c.ContactWebsiteAddress,
                        Email = c.ContactEmail
                    })
                }),
                LastSyncedFromCosmos = _clock.UtcNow
            }));

        private static Func<IReadOnlyCollection<T>, Task> GetSyncWithBatchingHandler<T>(
            Func<IReadOnlyCollection<T>, Task> processChunk) => async records =>
            {
                foreach (IReadOnlyCollection<T> c in records.Buffer(BatchSize))
                {
                    await Policy
                        .Handle<SqlException>()
                        .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(retry))
                        .ExecuteAsync(() => processChunk(c));
                }
            };

        private async Task WithExclusiveSqlLock(string lockName, Func<Task> action)
        {
            // Grab an exclusive lock inside a transaction that spans the entire duration of `action`'s execution.
            // ISqlQueryDispatcher owns the transaction; Dispose()ing the scope Dispose()s the transaction too.
            // Note that commiting this transaction is not necessary.
            // If the lock cannot be grabbed immediately then log & bail.

            var sqlDispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();

            var result = await sqlDispatcher.ExecuteQuery(new GetExclusiveLock()
            {
                Name = lockName,
                TimeoutMilliseconds = 0  // Return immediately if lock cannot be acquired
            });

            if (!result)
            {
                _logger.LogWarning($"Failed to acquire exclusive lock: '{lockName}'.");
                return;
            }

            await action();
        }

        private async Task WithSqlDispatcher(Func<ISqlQueryDispatcher, Task> action)
        {
            using (var sqlDispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                await action(sqlDispatcher);
                await sqlDispatcher.Commit();
            }
        }
    }
}
