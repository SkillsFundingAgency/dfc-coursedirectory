using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Testing;
using Microsoft.Extensions.DependencyInjection;
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
                Status = Models.ProviderStatus.Onboarded,
                ProviderType = Models.ProviderType.Both,
                ProviderName = "Test Provider",
                ProviderStatus = "Active",
                MarketingInformation = "Marketing information",
                CourseDirectoryName = "Another name",
                TradingName = "Trading name",
                Alias = "Alias",
                DateUpdated = Clock.UtcNow,
                UpdatedBy = "Tests",
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
                            PostCode = "AB1 2CD"
                        },
                        ContactTelephone1 = "01234 567890",
                        ContactFax = "02345 678901",
                        ContactWebsiteAddress = "https://provider.com/contact",
                        ContactEmail = "person@provider.com",
                        LastUpdated = Clock.UtcNow
                    }
                }
            };

            var sqlDataSync = new SqlDataSync(
                Fixture.Services.GetRequiredService<IServiceScopeFactory>(),
                CosmosDbQueryDispatcher.Object);

            // Act
            await sqlDataSync.SyncProvider(provider);

            // Assert
            Fixture.DatabaseFixture.SqlQuerySpy.VerifyQuery<UpsertProviders, None>(q =>
                q.Records.Any(p =>
                    p.ProviderId == provider.Id &&
                    p.Ukprn == provider.Ukprn &&
                    p.ProviderStatus == Models.ProviderStatus.Onboarded &&
                    p.ProviderType == Models.ProviderType.Both &&
                    p.UkrlpProviderStatusDescription == "Active" &&
                    p.MarketingInformation == "Marketing information" &&
                    p.CourseDirectoryName == "Another name" &&
                    p.TradingName == "Trading name" &&
                    p.Alias == "Alias" &&
                    p.UpdatedOn == Clock.UtcNow &&
                    p.UpdatedBy == "Tests"));
        }

        [Fact]
        public async Task SyncVenue_UpsertsVenue()
        {
            // Arrange
            var venue = new Venue()
            {
                Id = Guid.NewGuid(),
                Ukprn = 12345,
                VenueName = "Test",
                AddressLine1 = "Line 1",
                AddressLine2 = "Line 2",
                Town = "Town",
                County = "County",
                Postcode = "AB1 2DE",
                Latitude = 1,
                Longitude = 2,
                Telephone = "01234 567890",
                Email = "venue@provider.com",
                Website = "https://provider.com/venue",
                Status = 1,
                LocationId = 42,
                ProvVenueID = "MY VENUE",
                CreatedDate = Clock.UtcNow,
                CreatedBy = "Tests",
                DateUpdated = Clock.UtcNow,
                UpdatedBy = "Tests"
            };

            var sqlDataSync = new SqlDataSync(
                Fixture.Services.GetRequiredService<IServiceScopeFactory>(),
                CosmosDbQueryDispatcher.Object);

            // Act
            await sqlDataSync.SyncVenue(venue);

            // Assert
            Fixture.DatabaseFixture.SqlQuerySpy.VerifyQuery<UpsertVenues, None>(q =>
                q.Records.Any(v =>
                    v.VenueId == venue.Id &&
                    v.ProviderUkprn == 12345 &&
                    v.VenueName == "Test" &&
                    v.AddressLine1 == "Line 1" &&
                    v.AddressLine2 == "Line 2" &&
                    v.Town == "Town" &&
                    v.County == "County" &&
                    v.Postcode == "AB1 2DE" &&
                    v.Position.Latitude == 1 &&
                    v.Position.Longitude == 2 &&
                    v.Telephone == "01234 567890" &&
                    v.Email == "venue@provider.com" &&
                    v.Website == "https://provider.com/venue" &&
                    v.VenueStatus == Models.VenueStatus.Live &&
                    v.TribalVenueId == 42 &&
                    v.ProviderVenueRef == "MY VENUE" &&
                    v.CreatedBy == "Tests" &&
                    v.CreatedOn == Clock.UtcNow &&
                    v.UpdatedBy == "Tests" &&
                    v.UpdatedOn == Clock.UtcNow));
        }
    }
}
