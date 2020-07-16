using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Core
{
    public class SqlDataSync
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public SqlDataSync(
            IServiceScopeFactory serviceScopeFactory,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public async Task SyncAll()
        {
            await SyncAllProviders();
        }

        public Task SyncAllProviders() => _cosmosDbQueryDispatcher.ExecuteQuery(
            new ProcessAllProviders()
            {
                Process = SyncProvider
            });

        public Task SyncProvider(Provider provider) => WithSqlDispatcher(dispatcher =>
            dispatcher.ExecuteQuery(new UpsertProvider()
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
                Contacts = provider.ProviderContact.Select(c => new UpsertProviderContact()
                {
                    ContactType = c.ContactType,
                    ContactRole = c.ContactRole,
                    AddressSaonDescription = c.ContactAddress?.SAON?.Description,
                    AddressPaonDescription = c.ContactAddress?.PAON?.Description,
                    AddressStreetDescription = c.ContactAddress?.StreetDescription,
                    AddressLocality = c.ContactAddress?.Locality,
                    AddressItems = string.Join(" ", c.ContactAddress?.Items ?? Array.Empty<string>()),
                    AddressPostTown = c.ContactAddress?.PostTown,
                    AddressPostcode = c.ContactAddress?.PostCode,
                    PersonalDetailsPersonNameTitle = string.Join(" ", c.ContactPersonalDetails?.PersonNameTitle ?? Array.Empty<string>()),
                    PersonalDetailsPersonNameGivenName = string.Join(" ", c.ContactPersonalDetails?.PersonGivenName ?? Array.Empty<string>()),
                    PersonalDetailsPersonNameFamilyName = c.ContactPersonalDetails?.PersonFamilyName
                })
            }));

        private async Task WithSqlDispatcher(Func<ISqlQueryDispatcher, Task> action)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var sqlDispatcher = scope.ServiceProvider.GetRequiredService<ISqlQueryDispatcher>();

                await action(sqlDispatcher);

                sqlDispatcher.Transaction.Commit();
            }
        }
    }
}
