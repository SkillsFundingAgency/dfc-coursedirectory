using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
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
        public async Task SyncProviderData_ProviderDoesNotExist_CreatesProviderInfo()
        {
            // Arrange
            const int ukprn = 1234569;
            var ukrlpData = GenerateUkrlpProviderData(ukprn);
            var ukrlpContact = ukrlpData.ProviderContact.Single();
            var ukrlpSyncHelper = SetupUkrlpSyncHelper(ukprn, ukrlpData);

            ICollection<CreateProviderFromUkrlpData> capturedUpdateCommands = new List<CreateProviderFromUkrlpData>();
            await WithSqlQueryDispatcher(mock => mock.ExecuteQuery(Capture.In(capturedUpdateCommands)));

            // Act
            await ukrlpSyncHelper.SyncProviderData(ukprn);

            // Assert
            var createCommand = capturedUpdateCommands.Should().ContainSingle().Subject;
            createCommand.Alias.Should().Be(ukrlpData.ProviderAliases.Single().ProviderAlias);
            createCommand.DateUpdated.Should().Be(Clock.UtcNow);
            createCommand.ProviderName.Should().Be(ukrlpData.ProviderName);
            createCommand.ProviderStatus.Should().Be(ukrlpData.ProviderStatus);
            createCommand.ProviderType.Should().Be(ProviderType.None);
            createCommand.Status.Should().Be(ProviderStatus.Registered);
            createCommand.Ukprn.Should().Be(ukprn);
            var actualContact = createCommand.Contacts.Should().ContainSingle().Subject;
            AssertContactMapping(actualContact, ukrlpContact);
        }

        [Fact]
        public async Task SyncProviderData_ProviderAlreadyExists_UpdatesProviderInfo()
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerName: "Test Provider",
                providerType: ProviderType.FE,
                providerStatus: "Provider deactivated, not verified",
                contacts: new[]
                {
                    new ProviderContact
                    {
                        ContactType = "P",
                        Telephone1 = "0123456789",
                        WebsiteAddress = "http://www.example.com",
                        Email = "test@example.com",
                        AddressSaonDescription = "SAON Description",
                        AddressPaonDescription = "PAON Description",
                        AddressStreetDescription = "Street",
                        AddressLocality = "Locality",
                        AddressItems = "ItemDescription",
                        AddressPostTown = "PostTown",
                        AddressCounty = "County",
                        AddressPostcode = "Postcode",
                        PersonalDetailsPersonNameTitle = "Title",
                        PersonalDetailsPersonNameFamilyName = "FamilyName",
                        PersonalDetailsPersonNameGivenName = "GivenName"
                    }
                });

            var ukrlpData = GenerateUkrlpProviderData(provider.Ukprn);
            var ukrlpContact = ukrlpData.ProviderContact.Single();

            var ukrlpSyncHelper = SetupUkrlpSyncHelper(provider.Ukprn, ukrlpData);

            ICollection<UpdateProviderFromUkrlpData> capturedUpdateCommands = new List<UpdateProviderFromUkrlpData>();
            await WithSqlQueryDispatcher(mock => mock.ExecuteQuery(Capture.In(capturedUpdateCommands)));

            // Act
            await ukrlpSyncHelper.SyncProviderData(provider.Ukprn);

            // Assert
            var updateCommand = capturedUpdateCommands.Should().ContainSingle().Subject;
            updateCommand.Alias.Should().Be(ukrlpData.ProviderAliases.Single().ProviderAlias);
            updateCommand.DateUpdated.Should().Be(Clock.UtcNow);
            updateCommand.ProviderName.Should().Be(ukrlpData.ProviderName);
            updateCommand.ProviderId.Should().Be(provider.ProviderId);
            updateCommand.ProviderStatus.Should().Be(ukrlpData.ProviderStatus);
            var actualContact = updateCommand.Contacts.Should().ContainSingle().Subject;
            AssertContactMapping(actualContact, ukrlpContact);
        }

        [Fact]
        public void SelectContact_SelectsMostRecentlyUpdatedPTypeContact()
        {
            // Arrange
            var contact1 = new ProviderContactStructure
            {
                ContactType = "P",
                LastUpdated = new DateTime(2020, 3, 1),
                ContactAddress = new AddressStructure(),
                ContactPersonalDetails = new PersonNameStructure()
            };

            var contact2 = new ProviderContactStructure
            {
                ContactType = "P",
                LastUpdated = new DateTime(2020, 4, 1),
                ContactAddress = new AddressStructure(),
                ContactPersonalDetails = new PersonNameStructure()
            };

            var contact3 = new ProviderContactStructure
            {
                ContactType = "L",
                LastUpdated = new DateTime(2020, 5, 1),
                ContactAddress = new AddressStructure(),
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

        private void AssertContactMapping(ProviderContact actualContact, ProviderContactStructure ukrlpContact)
        {
            actualContact.ContactType.Should().Be(ukrlpContact.ContactType);
            actualContact.ContactRole.Should().Be(ukrlpContact.ContactRole);
            actualContact.Telephone1.Should().Be(ukrlpContact.ContactTelephone1);
            actualContact.WebsiteAddress.Should().Be(ukrlpContact.ContactWebsiteAddress);
            actualContact.Email.Should().Be(ukrlpContact.ContactEmail);
            actualContact.Fax.Should().Be(ukrlpContact.ContactFax);
            actualContact.PersonalDetailsPersonNameGivenName.Should().NotBeNull();

            actualContact.AddressPaonDescription.Should().Be(ukrlpContact.ContactAddress.Address2);
            actualContact.AddressSaonDescription.Should().Be(ukrlpContact.ContactAddress.Address1);
            actualContact.AddressStreetDescription.Should().Be(ukrlpContact.ContactAddress.Address3);
            actualContact.AddressLocality.Should().Be(ukrlpContact.ContactAddress.Address4);
            actualContact.AddressPostTown.Should().Be(ukrlpContact.ContactAddress.Town);
            actualContact.AddressCounty.Should().Be(ukrlpContact.ContactAddress.County);
            actualContact.AddressItems.Should().BeEquivalentTo(
            
                ukrlpContact.ContactAddress.Town +
                ukrlpContact.ContactAddress.County
            );

            actualContact.AddressPostcode.Should().Be(ukrlpContact.ContactAddress.PostCode);

            actualContact.PersonalDetailsPersonNameTitle.Should().Be(ukrlpContact.ContactPersonalDetails.PersonNameTitle.Single());
            actualContact.PersonalDetailsPersonNameGivenName.Should().Be(ukrlpContact.ContactPersonalDetails.PersonGivenName.Single());
            actualContact.PersonalDetailsPersonNameFamilyName.Should().Be(ukrlpContact.ContactPersonalDetails.PersonFamilyName);

        }

        private UkrlpSyncHelper SetupUkrlpSyncHelper(int ukprn, ProviderRecordStructure ukrlpProviderData)
        {
            var ukrlpWcfService = new Mock<IUkrlpService>();
            ukrlpWcfService
                .Setup(w => w.GetProviderData(It.Is<IEnumerable<int>>(v => v.SequenceEqual(new[] { ukprn }))))
                .ReturnsAsync(new Dictionary<int, ProviderRecordStructure>()
                {
                    { ukprn, ukrlpProviderData }
                });

            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(mock => mock.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());

            return new UkrlpSyncHelper(
                ukrlpWcfService.Object,
                SqlQueryDispatcherFactory.CreateDispatcher(IsolationLevel.Snapshot),
                Clock,
                loggerFactory.Object);
        }

        private ProviderRecordStructure GenerateUkrlpProviderData(int ukprn) => new ProviderRecordStructure
        {
            UnitedKingdomProviderReferenceNumber = ukprn.ToString(),
            ProviderName = "Ukrlp Test Provider",
            ProviderStatus = "Active",
            ProviderAliases = new[] { new ProviderAliasesStructure {ProviderAlias = "Ukrlp ProviderAliasHere"} },
            ProviderContact = new[]
            {
                new ProviderContactStructure
                {
                    ContactType = "P",
                    ContactAddress = new AddressStructure
                    {
                        Address1 = "ukrlp Address1",
                        Address2 = "ukrlp Address2",
                        Address3 = "ukrlp Address3",
                        Address4 = "ukrlp Address4",
                        Town = "ukrlp Town",
                        County = "ukrlp County",
                        PostCode = "Ukrlp PostCode",
                    },
                    ContactPersonalDetails = new PersonNameStructure
                    {
                        PersonNameTitle = new[] {"Ukrlp Mr"},
                        PersonGivenName = new[] {"Ukrlp Given name"},
                        PersonFamilyName = "Ukrlp Family name",
                    },
                    ContactTelephone1 = "Ukrlp Telephone 1",
                    ContactWebsiteAddress = "http://www.ukrlptest.example.com",
                    ContactEmail = "Ukrlptest@example.com",
                    LastUpdated = Clock.UtcNow,
                }
            }
        };
    }
}
