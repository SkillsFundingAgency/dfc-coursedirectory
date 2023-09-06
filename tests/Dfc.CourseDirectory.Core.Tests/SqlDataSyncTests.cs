using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using OneOf.Types;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests
{
    public class SqlDataSyncTests : DatabaseTestBase
    {
        public SqlDataSyncTests(DatabaseTestBaseFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public async Task SyncProvider_UpsertsProvider()
        {
            // Arrange
            var provider = new Provider()
            {
                Id = Guid.NewGuid(),
                UnitedKingdomProviderReferenceNumber = "12345",
                Status = ProviderStatus.Onboarded,
                ProviderType = ProviderType.FE,
                ProviderName = "Test Provider",
                ProviderStatus = "Active",
                MarketingInformation = "Marketing information",
                CourseDirectoryName = "Another name",
                TradingName = "Trading name",
                Alias = "Alias",
                DateUpdated = Clock.UtcNow,
                UpdatedBy = "Tests",
                ProviderId = 123456,
                BulkUploadStatus = new ProviderBulkUploadStatus
                {
                    InProgress = true,
                    PublishInProgress = false,
                    StartedTimestamp = DateTime.UtcNow,
                    TotalRowCount = 123
                },
                ProviderContact = new[]
                {
                    new ProviderContact()
                    {
                        ContactType = "P",
                        ContactRole = "Hero",
                        ContactPersonalDetails = new ProviderContactPersonalDetails()
                        {
                            PersonNameTitle = new[] { "Mr" },
                            PersonGivenName = new[] { "Person" },
                            PersonFamilyName = "Smith"
                        },
                        ContactAddress = new ProviderContactAddress()
                        {
                            SAON = new ProviderContactAddressSAON() { Description = "SAON" },
                            PAON = new ProviderContactAddressPAON() { Description = "PAON" },
                            StreetDescription = "Street",
                            Locality = "Locality",
                            Items = new[] { "Item1", "Item2" },
                            PostTown = "Town",
                            County = "County",
                            PostCode = "AB1 2CD"
                        },
                        ContactTelephone1 = "01234 567890",
                        ContactTelephone2 = "0345 678910",
                        ContactFax = "02345 678901",
                        ContactWebsiteAddress = "https://provider.com/contact",
                        ContactEmail = "person@provider.com",
                        LastUpdated = Clock.UtcNow
                    }
                }
            };


            // TODO: DO WE STILL NEED THIS ONE?
            //var sqlDataSync = new SqlDataSync(
            //    SqlQueryDispatcherFactory,
            //    CosmosDbQueryDispatcher.Object,
            //    Clock,
            //    new NullLogger<SqlDataSync>());

            // Act
            //await sqlDataSync.SyncProvider(provider);

            // Assert
            Fixture.DatabaseFixture.SqlQuerySpy.VerifyQuery<UpsertProvidersFromCosmos, None>(q =>
                q.LastSyncedFromCosmos == Clock.UtcNow &&
                q.Records.Any(p =>
                    p.ProviderId == provider.Id &&
                    p.Ukprn == provider.Ukprn &&
                    p.ProviderStatus == ProviderStatus.Onboarded &&
                    p.UkrlpProviderStatusDescription == "Active" &&
                    p.MarketingInformation == "Marketing information" &&
                    p.CourseDirectoryName == "Another name" &&
                    p.TradingName == "Trading name" &&
                    p.Alias == "Alias" &&
                    p.UpdatedOn == Clock.UtcNow &&
                    p.UpdatedBy == "Tests" &&
                    p.TribalProviderId == provider.ProviderId &&
                    p.BulkUploadInProgress == provider.BulkUploadStatus?.InProgress &&
                    p.BulkUploadPublishInProgress == provider.BulkUploadStatus?.PublishInProgress &&
                    p.BulkUploadStartedDateTime == provider.BulkUploadStatus?.StartedTimestamp &&
                    p.BulkUploadTotalRowCount == provider.BulkUploadStatus?.TotalRowCount &&
                    p.Contacts.Any(c =>
                        c.ContactType == "P" &&
                        c.ContactRole == "Hero" &&
                        c.AddressSaonDescription == "SAON" &&
                        c.AddressPaonDescription == "PAON" &&
                        c.AddressStreetDescription == "Street" &&
                        c.AddressLocality == "Locality" &&
                        c.AddressItems == "Item1 Item2" &&
                        c.AddressPostTown == "Town" &&
                        c.AddressCounty == "County" &&
                        c.AddressPostcode == "AB1 2CD" &&
                        c.PersonalDetailsPersonNameTitle == "Mr" &&
                        c.PersonalDetailsPersonNameGivenName == "Person" &&
                        c.PersonalDetailsPersonNameFamilyName == "Smith" &&
                        c.Telephone1 == "01234 567890" &&
                        c.Telephone2 == "0345 678910" &&
                        c.Fax == "02345 678901" &&
                        c.WebsiteAddress == "https://provider.com/contact" &&
                        c.Email == "person@provider.com"
                    )));
        }
    }
}
