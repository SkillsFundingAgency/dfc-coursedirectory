using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Features.Venues.AddVenue;
using FluentAssertions;
using FormFlow;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Venues.AddVenue
{
    public class PublishedTests : MvcTestBase
    {
        public PublishedTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_JourneyStateIsNotValid_ReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var addressLine1 = "Test Venue line 1";
            var addressLine2 = "Test Venue line 2";
            var town = "Town";
            var county = "County";
            var postcode = "AB1 2DE";
            var name = "My Venue";
            var email = "email@example.com";
            var phoneNumber = "020 7946 0000";
            var website = "example.com";

            var journeyInstance = CreateJourneyInstance(
                provider.ProviderId,
                new AddVenueJourneyModel()
                {
                    AddressLine1 = addressLine1,
                    AddressLine2 = addressLine2,
                    County = county,
                    Email = email,
                    Latitude = 42D,
                    Longitude = 43D,
                    Name = name,
                    AddressIsOutsideOfEngland = false,
                    Telephone = phoneNumber,
                    Postcode = postcode,
                    Town = town,
                    Website = website
                });

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/venues/add/success?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_ValidRequest_ReturnsExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var addressLine1 = "Test Venue line 1";
            var addressLine2 = "Test Venue line 2";
            var town = "Town";
            var county = "County";
            var postcode = "AB1 2DE";
            var name = "My Venue";
            var email = "email@example.com";
            var phoneNumber = "020 7946 0000";
            var website = "example.com";

            var journeyInstance = CreateJourneyInstance(
                provider.ProviderId,
                new AddVenueJourneyModel()
                {
                    AddressLine1 = addressLine1,
                    AddressLine2 = addressLine2,
                    County = county,
                    Email = email,
                    Latitude = 42D,
                    Longitude = 43D,
                    Name = name,
                    AddressIsOutsideOfEngland = false,
                    Telephone = phoneNumber,
                    Postcode = postcode,
                    Town = town,
                    Website = website
                });
            journeyInstance.Complete();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/venues/add/success?providerId={provider.ProviderId}&ffiid={journeyInstance.InstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("VenueName").TextContent.Should().Be(name);
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
