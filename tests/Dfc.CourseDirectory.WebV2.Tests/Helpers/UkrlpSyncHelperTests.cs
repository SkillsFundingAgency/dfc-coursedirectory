using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Helpers;
using Dfc.CourseDirectory.WebV2.Helpers.Interfaces;
using Dfc.CourseDirectory.WebV2.Security;
using Dfc.CourseDirectory.WebV2.Services.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UkrlpService;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.Helpers
{
    public class UkrlpSyncHelperTests : MvcTestBase
    {
        public UkrlpSyncHelperTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GivenSignInContext_WhenUkprnSuppliedAndProviderExists_EnsureProviderDataIsUpdated()
        {
            // Arrange
            int providerUkprn = 01234566;
            var providerData = this.GenerateProviderData(providerUkprn);

            var createProviderContacts = new List<CreateProviderContact>();

            providerData.Id = await TestData.CreateProvider(providerUkprn,
                                                            providerData.ProviderName,
                                                            providerData.ProviderType,
                                                            providerData.ProviderStatus,
                                                            ApprenticeshipQAStatus.Passed,
                                                            alias: providerData.Alias,
                                                            contacts: providerData.ProviderContact.ToList().Select(c => new CreateProviderContact()
                                                            {
                                                                ContactType = c.ContactType,
                                                                ContactTelephone1 = c.ContactTelephone1,
                                                                ContactWebsiteAddress = c.ContactWebsiteAddress,
                                                                ContactEmail = c.ContactEmail,
                                                                AddressSaonDescription = c.ContactAddress.SAON?.Description,
                                                                AddressPaonDescription = c.ContactAddress.PAON?.Description,
                                                                AddressStreetDescription = c.ContactAddress.StreetDescription,
                                                                AddressLocality = c.ContactAddress.Locality,
                                                                AddressItems = c.ContactAddress.Items,
                                                                AddressPostCode = c.ContactAddress.PostCode,
                                                                PersonalDetailsGivenName = c.ContactPersonalDetails?.PersonGivenName.FirstOrDefault(),
                                                                PersonalDetailsFamilyName = c.ContactPersonalDetails?.PersonFamilyName,
                                                            }));

            var signInContext = GetSignInContext(providerUkprn);
            var ukrlpProviderData = this.GenerateUkrlpProviderData(providerUkprn);
            var mockUkrlpWcfService = new Mock<IUkrlpWcfService>();
            mockUkrlpWcfService.Setup(w => w.GetProviderData(providerUkprn)).Returns(Task.FromResult(ukrlpProviderData));
            var mockCosmosDbQueryDispatcher = base.Factory.CosmosDbQueryDispatcher;
            var _ukrlpSyncHelper = new UkrlpSyncHelper(mockUkrlpWcfService.Object, mockCosmosDbQueryDispatcher.Object, base.Factory.Clock);

            // Act
            await _ukrlpSyncHelper.SyncProviderData(signInContext.Provider.Id, providerUkprn, signInContext.UserInfo.Email);

            // Assert
            var updatedProvider = mockCosmosDbQueryDispatcher.Object.ExecuteQuery(new GetProviderByUkprn() { Ukprn = providerUkprn }).Result;

            Assert.True(providerData.Id == updatedProvider.Id);
            Assert.True(ukrlpProviderData.ProviderName == updatedProvider.ProviderName);
            Assert.True(signInContext.UserInfo.Email == updatedProvider.UpdatedBy);
            Assert.True(ukrlpProviderData.UnitedKingdomProviderReferenceNumber == updatedProvider.UnitedKingdomProviderReferenceNumber);
            Assert.True(ukrlpProviderData.ProviderStatus == updatedProvider.ProviderStatus);
            Assert.True(ukrlpProviderData.ProviderContact.First().ContactAddress.Locality == updatedProvider.ProviderContact.First().ContactAddress.Locality);
            Assert.True(ukrlpProviderData.ProviderContact.First().ContactPersonalDetails.PersonFamilyName == updatedProvider.ProviderContact.First().ContactPersonalDetails.PersonFamilyName);

        }

        [Fact]
        public async Task GivenSignInContext_WhenUkprnSuppliedAndProviderExists_EnsureProviderDataIsCreated()
        {
            // Arrange
            int providerUkprn = 01234566;
            var providerData = this.GenerateProviderData(providerUkprn);
            var signInContext = GetSignInContext(providerUkprn);

            var ukrlpProviderData = this.GenerateUkrlpProviderData(providerUkprn);

            var mockUkrlpWcfService = new Mock<IUkrlpWcfService>();
            mockUkrlpWcfService.Setup(w => w.GetProviderData(providerUkprn)).Returns(Task.FromResult(ukrlpProviderData));
            var mockCosmosDbQueryDispatcher = base.Factory.CosmosDbQueryDispatcher;
            var _ukrlpSyncHelper = new UkrlpSyncHelper(mockUkrlpWcfService.Object, mockCosmosDbQueryDispatcher.Object, base.Factory.Clock);

            // Act
            await _ukrlpSyncHelper.SyncProviderData(signInContext.Provider.Id, providerUkprn, signInContext.UserInfo.Email);

            // Assert
            var updatedProvider = mockCosmosDbQueryDispatcher.Object.ExecuteQuery(new GetProviderByUkprn() { Ukprn = providerUkprn }).Result;

            Assert.True(ukrlpProviderData.ProviderName == updatedProvider.ProviderName);
            Assert.True(signInContext.UserInfo.Email == updatedProvider.UpdatedBy);
            Assert.True(ukrlpProviderData.UnitedKingdomProviderReferenceNumber == updatedProvider.UnitedKingdomProviderReferenceNumber);
            Assert.True(ukrlpProviderData.ProviderStatus == updatedProvider.ProviderStatus);
            Assert.True(ukrlpProviderData.ProviderContact.First().ContactAddress.Locality == updatedProvider.ProviderContact.First().ContactAddress.Locality);
            Assert.True(ukrlpProviderData.ProviderContact.First().ContactPersonalDetails.PersonFamilyName == updatedProvider.ProviderContact.First().ContactPersonalDetails.PersonFamilyName);
        }

        private Provider GenerateProviderData(int providerUkprn)
        {
            Provider provider = new Provider()
            {
                Id = Guid.NewGuid(),
                UnitedKingdomProviderReferenceNumber = providerUkprn.ToString(),
                ProviderName = "Test Provider",
                ProviderStatus = "Active",
                ProviderType = Core.Models.ProviderType.Both,
                ProviderContact = new List<ProviderContact>()
                {
                    new ProviderContact()
                    {
                        ContactType = "P",
                        ContactTelephone1 = "0123456789",
                        ContactFax = "0123456789",
                        ContactWebsiteAddress = "http://www.testing.com",
                        ContactEmail = "test@test.com",
                        LastUpdated = DateTime.Now,
                        ContactPersonalDetails = new ContactPersonalDetails()
                        {
                            PersonFamilyName = "Familynme",
                            PersonGivenName = new List<string>{ "GivenName"},
                            PersonNameTitle = new List<string>{ "Title"},
                        },
                        ContactAddress = new ContactAddress()
                        {
                            Items = new List<string>{ "ItemDescription" },
                            Locality = "Locality",
                            PAON  = new PAON(){ Description = "PAON Description"},
                            SAON  = new SAON(){ Description = "PAON Description"},
                            PostCode = "Postcode",
                            StreetDescription = "Street"

                        }
                    }
                }
            };

            return provider;
        }

        private SignInContext GetSignInContext(int providerUkprn)
        {
            var signInContext = new SignInContext(this.Factory.User.ToPrincipal());
            signInContext.DfeSignInOrganisationId = Guid.NewGuid().ToString();

            // add provider data
            var provider = GenerateProviderData(providerUkprn);
            signInContext.ProviderUkprn = providerUkprn;
            signInContext.Provider = provider;

            // add userInfo data
            signInContext.UserInfo = new AuthenticatedUserInfo()
            {
                Email = "testuser@test.com",
                FirstName = "First Name",
                LastName = "Last Name",
            };

            return signInContext;
        }

        private ProviderRecordStructure GenerateUkrlpProviderData(int ukprn)
        {
            var contactAddress = new BSaddressStructure();
            contactAddress.SAON = new AONstructure() { Description = "Ukrlp SAON Description" };
            contactAddress.PAON = new AONstructure() { Description = "Ukrlp PAON Description" };
            contactAddress.StreetDescription = "Ukrlp Street";
            contactAddress.Locality = "Ukrlp Locality";
            contactAddress.Items = new string[] { "Ukrlp Item Description " };
            contactAddress.PostCode = "Ukrlp PostCode";
            contactAddress.PostCode = "Ukrlp PostCode";

            var contactPersonalDetails = new PersonNameStructure();
            contactPersonalDetails.PersonNameTitle = new string[] { "Ukrlp Mr" };
            contactPersonalDetails.PersonGivenName = new string[] { "Ukrlp Given name" };
            contactPersonalDetails.PersonFamilyName = "Ukrlp Family name";

            return new ProviderRecordStructure()
            {
                UnitedKingdomProviderReferenceNumber = ukprn.ToString(),
                ProviderName = "Ukrlp Test Provider",
                ProviderStatus = "Active",
                ProviderAliases = new ProviderAliasesStructure[] { new ProviderAliasesStructure() { ProviderAlias = "ProviderAlias" } },

                ProviderContact = new ProviderContactStructure[]
                {
                    new ProviderContactStructure() {
                            ContactType = "P",
                            ContactAddress = contactAddress,
                            ContactPersonalDetails = contactPersonalDetails,
                            ContactTelephone1 = "Ukrlp Telephone 1",
                            ContactWebsiteAddress = "http://www.ukrlptest.com",
                            ContactEmail = "Ukrlptest@test.com   ",
                            LastUpdated = DateTime.Now,
                    }
                }
            };
        }
    }
}
