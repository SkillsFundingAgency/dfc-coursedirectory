using System;
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

            var sqlDataSync = new SqlDataSync(Fixture.Services.GetRequiredService<IServiceScopeFactory>());

            // Act
            await sqlDataSync.SyncProvider(provider);

            // Assert
            Fixture.DatabaseFixture.SqlQuerySpy.VerifyQuery<UpsertProvider, None>(q =>
                q.ProviderId == provider.Id &&
                q.Ukprn == provider.Ukprn &&
                q.ProviderStatus == Models.ProviderStatus.Onboarded &&
                q.ProviderType == Models.ProviderType.Both &&
                q.UkrlpProviderStatusDescription == "Active" &&
                q.MarketingInformation == "Marketing information" &&
                q.CourseDirectoryName == "Another name" &&
                q.TradingName == "Trading name" &&
                q.Alias == "Alias" &&
                q.UpdatedOn == Clock.UtcNow &&
                q.UpdatedBy == "Tests");
        }
    }
}
