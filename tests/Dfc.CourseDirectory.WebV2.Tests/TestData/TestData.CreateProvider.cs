using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.Queries;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
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

        //public async Task<Guid> CreateProvider(Provider providerToAdd)
        //{
        //    var createProviderContacts = providerToAdd.ProviderContact?.Select(c => new CreateProviderContact()
        //    {
        //        ContactType = c.ContactType,
        //        ContactTelephone1 = c.ContactTelephone1,
        //        ContactWebsiteAddress = c.ContactWebsiteAddress,
        //        ContactEmail = c.ContactEmail,
        //        AddressSaonDescription = c.ContactAddress.SAON.Description,
        //        AddressPaonDescription = c.ContactAddress.PAON.Description,
        //        AddressLocality = c.ContactAddress.Locality,
        //        AddressItems = c.ContactAddress.Items,
        //        AddressPostCode = c.ContactAddress.PostCode,
        //        PersonalDetailsFamilyName = c.ContactPersonalDetails.PersonFamilyName,
        //        PersonalDetailsGivenName = c.ContactPersonalDetails.PersonGivenName.FirstOrDefault(),
        //    });

        //    return await this.CreateProvider(providerToAdd.Ukprn, 
        //                                        providerToAdd.ProviderName, 
        //                                        providerToAdd.ProviderType, 
        //                                        providerToAdd.ProviderStatus, 
        //                                        ApprenticeshipQAStatus.Passed, 
        //                                        providerToAdd.MarketingInformation, 
        //                                        providerToAdd.CourseDirectoryName, 
        //                                        providerToAdd.Alias, 
        //                                        createProviderContacts);
        //}
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
