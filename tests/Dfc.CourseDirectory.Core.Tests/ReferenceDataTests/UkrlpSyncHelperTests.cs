using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using UkrlpService;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.ReferenceDataTests
{
    public class UkrlpSyncHelperTests : DatabaseTestBase
    {
        public UkrlpSyncHelperTests(DatabaseTestBaseFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public async Task SyncProviderData_ProviderAlreadyExists_UpdatesProviderInfo()
        {
            // Arrange
            var ukprn = 01234566;

            var providerId = await TestData.CreateProvider(
                ukprn,
                providerName: "Test Provider",
                providerType: ProviderType.Both,
                providerStatus: "Active",
                contacts: new[]
                {
                    new CreateProviderContact()
                    {
                        ContactType = "P",
                        ContactTelephone1 = "0123456789",
                        ContactWebsiteAddress = "http://www.testing.com",
                        ContactEmail = "test@test.com",
                        AddressItems = new[] { "ItemDescription" },
                        AddressPaonDescription = "PAON Description",
                        AddressSaonDescription = "SAON Description",
                        AddressStreetDescription = "Street",
                        AddressPostCode = "Postcode",
                        AddressLocality = "Locality",
                        PersonalDetailsFamilyName = "FamilyName",
                        PersonalDetailsGivenName = "GivenName"
                    }
                });

            var ukrlpProviderData = GenerateUkrlpProviderData(ukprn);

            var ukrlpWcfService = new Mock<IUkrlpService>();
            ukrlpWcfService.Setup(w => w.GetProviderData(ukprn)).ReturnsAsync(ukrlpProviderData);

            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(mock => mock.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());

            var ukrlpSyncHelper = new UkrlpSyncHelper(
                ukrlpWcfService.Object,
                CosmosDbQueryDispatcher.Object,
                Clock,
                loggerFactory.Object);

            // Act
            await ukrlpSyncHelper.SyncProviderData(ukprn);

            // Assert
            CosmosDbQueryDispatcher.Verify(mock => mock.ExecuteQuery(It.Is<UpdateProviderFromUkrlpData>(cmd =>
                cmd.Alias == "ProviderAlias" &&
                cmd.DateUpdated == Clock.UtcNow &&
                cmd.ProviderId == providerId &&
                cmd.ProviderName == ukrlpProviderData.ProviderName &&
                cmd.ProviderStatus == "Active"
            )));
        }

        [Fact]
        public async Task SyncProviderData_ProviderDoesNotExist_CreatesProviderInfo()
        {
            // Arrange
            var ukprn = 01234566;

            var ukrlpProviderData = GenerateUkrlpProviderData(ukprn);

            var ukrlpWcfService = new Mock<IUkrlpService>();
            ukrlpWcfService.Setup(w => w.GetProviderData(ukprn)).ReturnsAsync(ukrlpProviderData);

            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(mock => mock.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());

            var ukrlpSyncHelper = new UkrlpSyncHelper(
                ukrlpWcfService.Object,
                CosmosDbQueryDispatcher.Object,
                Clock,
                loggerFactory.Object);

            // Act
            await ukrlpSyncHelper.SyncProviderData(ukprn);

            // Assert
            CosmosDbQueryDispatcher.Verify(mock => mock.ExecuteQuery(It.Is<CreateProviderFromUkrlpData>(cmd =>
                cmd.Alias == "ProviderAlias" &&
                cmd.DateUpdated == Clock.UtcNow &&
                cmd.ProviderName == ukrlpProviderData.ProviderName &&
                cmd.ProviderStatus == "Active" &&
                cmd.ProviderType == ProviderType.Both &&
                cmd.Status == ProviderStatus.Registered &&
                cmd.Ukprn == ukprn
            )));
        }

        [Fact]
        public void SelectContact_SelectsMostRecentlyUpdatedPTypeContact()
        {
            // Arrange

            var contact1 = new ProviderContactStructure()
            {
                ContactType = "P",
                LastUpdated = new DateTime(2020, 3, 1),
                ContactAddress = new BSaddressStructure(),
                ContactPersonalDetails = new PersonNameStructure()
            };

            var contact2 = new ProviderContactStructure()
            {
                ContactType = "P",
                LastUpdated = new DateTime(2020, 4, 1),
                ContactAddress = new BSaddressStructure(),
                ContactPersonalDetails = new PersonNameStructure()
            };

            var contact3 = new ProviderContactStructure()
            {
                ContactType = "L",
                LastUpdated = new DateTime(2020, 5, 1),
                ContactAddress = new BSaddressStructure(),
                ContactPersonalDetails = new PersonNameStructure()
            };

            var contacts = new[]
            {
                contact1,
                contact2,
                contact3
            };

            // Act
            var selected = UkrlpSyncHelper.SelectContact(contacts);

            // Assert
            Assert.Same(selected, contact2);
        }

        private ProviderRecordStructure GenerateUkrlpProviderData(int ukprn) => new ProviderRecordStructure()
        {
            UnitedKingdomProviderReferenceNumber = ukprn.ToString(),
            ProviderName = "Ukrlp Test Provider",
            ProviderStatus = "A",
            ProviderAliases = new[]
            {
                new ProviderAliasesStructure() { ProviderAlias = "ProviderAlias" }
            },
            ProviderContact = new ProviderContactStructure[]
            {
                new ProviderContactStructure()
                {
                    ContactType = "P",
                    ContactAddress = new BSaddressStructure()
                    {
                        SAON = new AONstructure() { Description = "Ukrlp SAON Description" },
                        PAON = new AONstructure() { Description = "Ukrlp PAON Description" },
                        StreetDescription = "Ukrlp Street",
                        Locality = "Ukrlp Locality",
                        Items = new string[] { "Ukrlp Item Description " },
                        PostCode = "Ukrlp PostCode"
                    },
                    ContactPersonalDetails = new PersonNameStructure()
                    {
                        PersonNameTitle = new string[] { "Ukrlp Mr" },
                        PersonGivenName = new string[] { "Ukrlp Given name" },
                        PersonFamilyName = "Ukrlp Family name",
                    },
                    ContactTelephone1 = "Ukrlp Telephone 1",
                    ContactWebsiteAddress = "http://www.ukrlptest.com",
                    ContactEmail = "Ukrlptest@test.com",
                    LastUpdated = Clock.UtcNow,
                }
            }
        };
    }
}
