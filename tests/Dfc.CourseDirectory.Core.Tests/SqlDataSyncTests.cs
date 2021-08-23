using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Microsoft.Extensions.DependencyInjection;
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
                ProviderType = ProviderType.FE | ProviderType.Apprenticeships,
                ProviderName = "Test Provider",
                ProviderStatus = "Active",
                MarketingInformation = "Marketing information",
                CourseDirectoryName = "Another name",
                TradingName = "Trading name",
                Alias = "Alias",
                DateUpdated = Clock.UtcNow,
                UpdatedBy = "Tests",
                NationalApprenticeshipProvider = true,
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

            var sqlDataSync = new SqlDataSync(
                SqlQueryDispatcherFactory,
                CosmosDbQueryDispatcher.Object,
                Clock,
                new NullLogger<SqlDataSync>());

            // Act
            await sqlDataSync.SyncProvider(provider);

            // Assert
            Fixture.DatabaseFixture.SqlQuerySpy.VerifyQuery<UpsertProvidersFromCosmos, None>(q =>
                q.LastSyncedFromCosmos == Clock.UtcNow &&
                q.Records.Any(p =>
                    p.ProviderId == provider.Id &&
                    p.Ukprn == provider.Ukprn &&
                    p.ProviderStatus == ProviderStatus.Onboarded &&
                    p.ProviderType == (ProviderType.FE | ProviderType.Apprenticeships) &&
                    p.UkrlpProviderStatusDescription == "Active" &&
                    p.MarketingInformation == "Marketing information" &&
                    p.CourseDirectoryName == "Another name" &&
                    p.TradingName == "Trading name" &&
                    p.Alias == "Alias" &&
                    p.UpdatedOn == Clock.UtcNow &&
                    p.UpdatedBy == "Tests" &&
                    p.NationalApprenticeshipProvider == provider.NationalApprenticeshipProvider &&
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

        [Fact]
        public async Task SyncApprenticeship_UpsertsApprenticeship()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var apprenticeship = new Apprenticeship()
            {
                Id = Guid.NewGuid(),
                ProviderId = provider.ProviderId,
                ProviderUKPRN = provider.Ukprn,
                ApprenticeshipTitle = "Test Apprenticeship",
                ApprenticeshipType = ApprenticeshipType.StandardCode,
                StandardId = Guid.NewGuid(),
                StandardCode = 123,
                Version = 2,
                NotionalNVQLevelv2 = "3",
                MarketingInformation = "Our amazing apprenticeship",
                Url = "https://provider.com/apprenticeship",
                ContactTelephone = "01234 567890",
                ContactEmail = "apprenticeship@provider.com",
                ContactWebsite = "https://provider.com",
                BulkUploadErrors = new List<BulkUploadError>
                {
                    new BulkUploadError { Error = "TestBulkUploadError1" },
                    new BulkUploadError { Error = "TestBulkUploadError2" },
                    new BulkUploadError { Error = "TestBulkUploadError3" }
                },
                ApprenticeshipLocations = new List<ApprenticeshipLocation>()
                {
                    new ApprenticeshipLocation()
                    {
                        Id = Guid.NewGuid(),
                        VenueId = Guid.NewGuid(),
                        National = false,
                        DeliveryModes = new List<ApprenticeshipDeliveryMode>()
                        {
                            ApprenticeshipDeliveryMode.EmployerAddress,
                            ApprenticeshipDeliveryMode.DayRelease
                        },
                        Name = "The Place",
                        Phone = "01234 567890",
                        ProviderUKPRN = 12345,
                        Regions = new[] { "E12000001" },  // North East
                        ApprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBasedAndEmployerBased,
                        LocationType = LocationType.Venue,
                        Radius = 30,
                        RecordStatus = 1,
                        CreatedDate = Clock.UtcNow,
                        CreatedBy = "Tests",
                        UpdatedDate = Clock.UtcNow,
                        UpdatedBy = "Tests",
                        ApprenticeshipLocationId = 689
                    }
                },
                RecordStatus = (int)ApprenticeshipStatus.Live,
                CreatedDate = Clock.UtcNow,
                CreatedBy = "Tests",
                UpdatedDate = Clock.UtcNow,
                UpdatedBy = "Tests",
                ApprenticeshipId = 56789
            };

            var sqlDataSync = new SqlDataSync(
                SqlQueryDispatcherFactory,
                CosmosDbQueryDispatcher.Object,
                Clock,
                new NullLogger<SqlDataSync>());

            // Act
            await sqlDataSync.SyncApprenticeship(apprenticeship);

            // Assert
            Fixture.DatabaseFixture.SqlQuerySpy.VerifyQuery<UpsertApprenticeshipsFromCosmos, None>(q =>
            {
                if (q.LastSyncedFromCosmos != Clock.UtcNow)
                {
                    return false;
                }

                var record = q.Records.SingleOrDefault();
                if (record == default)
                {
                    return false;
                }

                var recordLocation = record.Locations.SingleOrDefault();
                if (recordLocation == default)
                {
                    return false;
                }

                return record.ApprenticeshipId == apprenticeship.Id &&
                    record.ProviderId == apprenticeship.ProviderId &&
                    record.ProviderUkprn == provider.Ukprn &&
                    record.ApprenticeshipTitle == "Test Apprenticeship" &&
                    record.ApprenticeshipType == ApprenticeshipType.StandardCode &&
                    record.StandardCode == 123 &&
                    record.StandardVersion == 2 &&
                    record.MarketingInformation == "Our amazing apprenticeship" &&
                    record.ApprenticeshipWebsite == "https://provider.com/apprenticeship" &&
                    record.ContactTelephone == "01234 567890" &&
                    record.ContactEmail == "apprenticeship@provider.com" &&
                    record.ContactWebsite == "https://provider.com" &&
                    record.BulkUploadErrorCount == 3 &&
                    recordLocation.ApprenticeshipLocationId == apprenticeship.ApprenticeshipLocations.Single().Id &&
                    recordLocation.VenueId == apprenticeship.ApprenticeshipLocations.Single().VenueId &&
                    recordLocation.National == false &&
                    recordLocation.DeliveryModes.Count() == 2 &&
                    recordLocation.DeliveryModes.Contains(ApprenticeshipDeliveryMode.EmployerAddress) &&
                    recordLocation.DeliveryModes.Contains(ApprenticeshipDeliveryMode.DayRelease) &&
                    recordLocation.Name == "The Place" &&
                    recordLocation.Telephone == "01234 567890" &&
                    recordLocation.Regions.Single() == "E12000001" &&
                    recordLocation.ApprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBasedAndEmployerBased &&
                    recordLocation.LocationType == LocationType.Venue &&
                    recordLocation.Radius == 30 &&
                    recordLocation.TribalApprenticeshipLocationId == 689;
            });
        }
    }
}
