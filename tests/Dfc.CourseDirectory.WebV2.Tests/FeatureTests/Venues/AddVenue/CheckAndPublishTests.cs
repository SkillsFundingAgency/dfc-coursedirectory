using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.Venues.AddVenue;
using FluentAssertions;
using FluentAssertions.Execution;
using FormFlow;
using OneOf.Types;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Venues.AddVenue
{
    public class CheckAndPublishTests : MvcTestBase
    {
        public CheckAndPublishTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Get_ReturnsExpectedOutput(bool addressIsOutsideOfEngland)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();

            var addressLine1 = "Test Venue line 1";
            var addressLine2 = "Test Venue line 2";
            var town = "Town";
            var county = "County";
            var postcode = "AB1 2DE";
            var name = "My Venue";
            var email = "email@example.com";
            var telephone = "020 7946 0000";
            var website = "example.com";

            var journeyInstance = CreateJourneyInstance(
                providerId,
                new AddVenueJourneyModel()
                {
                    AddressLine1 = addressLine1,
                    AddressLine2 = addressLine2,
                    County = county,
                    Email = email,
                    Latitude = 42M,
                    Longitude = 43M,
                    Name = name,
                    AddressIsOutsideOfEngland = addressIsOutsideOfEngland,
                    Telephone = telephone,
                    Postcode = postcode,
                    Town = town,
                    Website = website,
                    ValidStages = AddVenueCompletedStages.All
                });

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/venues/add/check-publish?providerId={providerId}&ffiid={journeyInstance.InstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (new AssertionScope())
            {
                var doc = await response.GetDocument();
                doc.GetSummaryListValueWithKey("Location name").Should().Be(name);
                doc.GetSummaryListValueWithKey("Address").Should().Be($"{addressLine1}\n{addressLine2}\n{town}\n{county}\n{postcode}");
                doc.GetSummaryListValueWithKey("Email").Should().Be(email);
                doc.GetSummaryListValueWithKey("Telephone").Should().Be(telephone);
                doc.GetSummaryListValueWithKey("Website").Should().Be(website);

                if (addressIsOutsideOfEngland)
                {
                    doc.GetElementByTestId("outsideofengland-notification").Should().NotBeNull();
                }
                else
                {
                    doc.GetElementByTestId("outsideofengland-notification").Should().BeNull();
                }
            }
        }

        [Fact]
        public async Task Post_ValidRequest_CreatesVenueCompletesJourneyAndRedirects()
        {
            // Arrange
            var ukprn = 12345;
            var providerId = await TestData.CreateProvider(ukprn: ukprn);

            var addressLine1 = "Test Venue line 1";
            var addressLine2 = "Test Venue line 2";
            var town = "Town";
            var county = "County";
            var postcode = "AB1 2DE";
            var name = "My Venue";
            var email = "email@example.com";
            var telephone = "020 7946 0000";
            var website = "example.com";
            var latitude = 42M;
            var longitude = 43M;

            var journeyInstance = CreateJourneyInstance(
                providerId,
                new AddVenueJourneyModel()
                {
                    AddressLine1 = addressLine1,
                    AddressLine2 = addressLine2,
                    County = county,
                    Email = email,
                    Latitude = latitude,
                    Longitude = longitude,
                    Name = name,
                    AddressIsOutsideOfEngland = false,
                    Telephone = telephone,
                    Postcode = postcode,
                    Town = town,
                    Website = website,
                    ValidStages = AddVenueCompletedStages.All
                });

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/venues/add/publish?providerId={providerId}&ffiid={journeyInstance.InstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);

            response.Headers.Location.OriginalString.Should().Be(
                $"/venues/add/success?providerId={providerId}&ffiid={journeyInstance.InstanceId.UniqueKey}");

            GetJourneyInstance<AddVenueJourneyModel>(journeyInstance.InstanceId).Completed.Should().BeTrue();

            CosmosDbQueryDispatcher.VerifyExecuteQuery<CreateVenue, Success>(q =>
                q.ProviderUkprn == ukprn &&
                q.Name == name &&
                q.Email == email &&
                q.Telephone == telephone &&
                q.Website == website &&
                q.AddressLine1 == addressLine1 &&
                q.AddressLine2 == addressLine2 &&
                q.Town == town &&
                q.County == county &&
                q.Postcode == postcode &&
                q.Latitude == latitude &&
                q.Longitude == longitude &&
                q.CreatedBy.UserId == User.UserId &&
                q.CreatedDate == Clock.UtcNow);
        }

        private JourneyInstance<AddVenueJourneyModel> CreateJourneyInstance(Guid providerId, AddVenueJourneyModel state)
        {
            state ??= new AddVenueJourneyModel();

            var uniqueKey = Guid.NewGuid().ToString();

            return CreateJourneyInstance(
                journeyName: "AddVenue",
                configureKeys: keysBuilder => keysBuilder.With("providerId", providerId),
                state,
                uniqueKey: uniqueKey);
        }
    }
}
