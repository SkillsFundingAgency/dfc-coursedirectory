using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing.DataStore.CosmosDb.Queries;
using Xunit;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<Guid> CreateProvider(
            int ukprn = 12345,
            string providerName = "Test Provider",
            ProviderType providerType = ProviderType.Both,
            string providerStatus = "Active",
            ApprenticeshipQAStatus apprenticeshipQAStatus = ApprenticeshipQAStatus.Passed,
            string marketingInformation = "",
            string courseDirectoryName = "",
            string alias = "",
            IEnumerable<CreateProviderContact> contacts = null)
        {
            var providerId = Guid.NewGuid();

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
                    ContactAddress = new ContactAddress()
                    {
                        Items = c.AddressItems,
                        Locality = c.AddressLocality,
                        PAON = new PAON() { Description = c.AddressPaonDescription },
                        SAON = new SAON() { Description = c.AddressSaonDescription },
                        PostCode = c.AddressPostCode,
                        StreetDescription = c.AddressStreetDescription
                    },
                    ContactPersonalDetails = new ContactPersonalDetails()
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
            });
            Assert.Equal(CreateProviderResult.Ok, result);

            await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new SetProviderApprenticeshipQAStatus()
                {
                    ProviderId = providerId,
                    ApprenticeshipQAStatus = apprenticeshipQAStatus
                }));

            return providerId;
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
        public string AddressPostCode { get; set; }
        public string PersonalDetailsGivenName { get; set; }
        public string PersonalDetailsFamilyName { get; set; }
    }
}
