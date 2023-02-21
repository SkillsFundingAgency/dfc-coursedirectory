using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing.DataStore.CosmosDb.Queries;
using Xunit;
using Provider = Dfc.CourseDirectory.Core.DataStore.Sql.Models.Provider;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<Provider> CreateProvider(
            string providerName = "Test Provider",
            ProviderType providerType = ProviderType.FE,
            string providerStatus = "Active",
            ApprenticeshipQAStatus? apprenticeshipQAStatus = ApprenticeshipQAStatus.UnableToComplete,
            string marketingInformation = "",
            string courseDirectoryName = "",
            string alias = "",
            ProviderDisplayNameSource displayNameSource = default,
            IEnumerable<CreateProviderContact> contacts = null,
            IReadOnlyCollection<Guid> tLevelDefinitionIds = null,
            ProviderStatus status = ProviderStatus.Onboarded)
        {
            if (!providerType.HasFlag(ProviderType.TLevels) &&
                (tLevelDefinitionIds?.Count ?? 0) != 0)
            {
                throw new ArgumentException(
                    $"{nameof(tLevelDefinitionIds)} can only be specified for T Level providers.",
                    nameof(tLevelDefinitionIds));
            }

            var providerId = Guid.NewGuid();
            var ukprn = _uniqueIdHelper.GenerateProviderUkprn();

            var result = await _cosmosDbQueryDispatcher.ExecuteQuery(new CreateProvider()
            {
                ProviderId = providerId,
                Ukprn = ukprn,
                ProviderType = providerType,
                ProviderName = providerName,
                ProviderStatus = providerStatus,
                MarketingInformation = marketingInformation,
                CourseDirectoryName = courseDirectoryName,
                Alias = alias,
                ProviderContact = contacts?.Select(c => new ProviderContact()
                {
                    ContactAddress = new ProviderContactAddress()
                    {
                        Items = c.AddressItems,
                        Locality = c.AddressLocality,
                        PAON = new ProviderContactAddressPAON() { Description = c.AddressPaonDescription },
                        SAON = new ProviderContactAddressSAON() { Description = c.AddressSaonDescription },
                        PostTown = c.AddressPostTown,
                        County = c.AddressCounty,
                        PostCode = c.AddressPostCode,
                        StreetDescription = c.AddressStreetDescription
                    },
                    ContactPersonalDetails = new ProviderContactPersonalDetails()
                    {
                        PersonGivenName = new[] { c.PersonalDetailsGivenName },
                        PersonFamilyName = c.PersonalDetailsFamilyName
                    },
                    ContactEmail = c.ContactEmail,
                    ContactTelephone1 = c.ContactTelephone1,
                    ContactType = c.ContactType,
                    ContactWebsiteAddress = c.ContactWebsiteAddress,
                    LastUpdated = _clock.UtcNow
                }),
                DateUpdated = _clock.UtcNow,
                UpdatedBy = "TestData",
                Status = status
            });
            Assert.Equal(CreateProviderResult.Ok, result);

            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new Core.DataStore.CosmosDb.Queries.GetProviderById() { ProviderId = providerId });
            await _sqlDataSync.SyncProvider(provider);

            if (apprenticeshipQAStatus.HasValue)
            {
                await WithSqlQueryDispatcher(
                    dispatcher => dispatcher.ExecuteQuery(new SetProviderApprenticeshipQAStatus()
                    {
                        ProviderId = providerId,
                        ApprenticeshipQAStatus = apprenticeshipQAStatus.Value
                    }));
            }

            if (displayNameSource != default)
            {
                await WithSqlQueryDispatcher(
                    dispatcher => dispatcher.ExecuteQuery(new SetProviderDisplayNameSource()
                    {
                        ProviderId = providerId,
                        DisplayNameSource = displayNameSource
                    }));
            }

            if ((tLevelDefinitionIds?.Count ?? 0) != 0)
            {
                await SetProviderTLevelDefinitions(providerId, tLevelDefinitionIds);
            }

            return await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetProviderById() { ProviderId = providerId }));
        }
    }

    public class CreateProviderContact
    {
        public string ContactType { get; set; }
        public string ContactTelephone1 { get; set; }
        public string ContactWebsiteAddress { get; set; }
        public string ContactEmail { get; set; }
        public string AddressSaonDescription { get; set; }
        public string AddressPaonDescription { get; set; }
        public string AddressStreetDescription { get; set; }
        public string AddressLocality { get; set; }
        public IList<string> AddressItems { get; set; }
        public string AddressPostTown { get; set; }
        public string AddressCounty { get; set; }
        public string AddressPostCode { get; set; }
        public string PersonalDetailsGivenName { get; set; }
        public string PersonalDetailsFamilyName { get; set; }
    }
}
