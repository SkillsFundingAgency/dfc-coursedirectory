using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;
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
            string alias = "",
            ProviderDisplayNameSource displayNameSource = default,
            IReadOnlyCollection<Guid> tLevelDefinitionIds = null,
            ProviderStatus status = ProviderStatus.Onboarded,
            ProviderContact[] contacts = null,
            string marketingInformation = null)
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

            var result = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new CreateProviderFromUkrlpData()
            {
                ProviderId = providerId,
                Ukprn = ukprn,
                ProviderType = providerType,
                ProviderName = providerName,
                ProviderStatus = providerStatus,
                Alias = alias,
                Contacts = contacts?.Select(c => new ProviderContact()
                {
                    AddressItems = c.AddressItems,
                    AddressLocality = c.AddressLocality,
                    AddressPaonDescription = c.AddressPaonDescription ,
                    AddressSaonDescription = c.AddressSaonDescription ,
                    AddressPostTown = c.AddressPostTown,
                    AddressCounty = c.AddressCounty,
                    AddressPostcode = c.AddressPostcode,
                    AddressStreetDescription = c.AddressStreetDescription,
                    PersonalDetailsPersonNameTitle = c.PersonalDetailsPersonNameTitle,
                    PersonalDetailsPersonNameGivenName = c.PersonalDetailsPersonNameGivenName,
                    PersonalDetailsPersonNameFamilyName = c.PersonalDetailsPersonNameFamilyName,
                    Email = c.Email,
                    Telephone1 = c.Telephone1,
                    ContactType = c.ContactType,
                    WebsiteAddress = c.WebsiteAddress
                }),
                DateUpdated = _clock.UtcNow,
                UpdatedBy = "TestData",
                Status = status
            }));
            Assert.Equal(new Success(), result);

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
}
