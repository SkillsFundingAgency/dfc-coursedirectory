using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.Venues.EditVenue;
using FluentAssertions;
using FormFlow;
using Microsoft.Extensions.DependencyInjection;
using OneOf;
using OneOf.Types;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Venues.EditVenue
{
    public class SaveTests : MvcTestBase
    {
        public SaveTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Post_UserCannotAccessVenue_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(ukprn: 12345);
            var venueId = (await TestData.CreateVenue(providerId, email: "person@provider.com")).Id;

            var anotherProviderId = await TestData.CreateProvider(ukprn: 67890);

            var journeyInstance = await CreateJourneyInstance(venueId);
            journeyInstance.UpdateState(state =>
            {
                state.Name = "Updated name";
                state.Email = "updated@provider.com";
                state.PhoneNumber = "02345 678901";
                state.Website = "updated-provider.com";
                state.AddressLine1 = "Updated line 1";
                state.AddressLine2 = "Updated line 2";
                state.Town = "Updated town";
                state.County = "Updated county";
                state.Postcode = "UP1 D8D";
                state.Latitude = 42;
                state.Longitude = 42;
            });

            var requestContent = new FormUrlEncodedContentBuilder()
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}")
            {
                Content = requestContent
            };

            await User.AsTestUser(userType, anotherProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_VenueDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var venueId = Guid.NewGuid();

            var requestContent = new FormUrlEncodedContentBuilder()
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_ValidRequest_UpdatesDatabaseAndRedirects()
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var journeyInstance = await CreateJourneyInstance(venueId);
            journeyInstance.UpdateState(state =>
            {
                state.Name = "Updated name";
                state.Email = "updated@provider.com";
                state.PhoneNumber = "02345 678901";
                state.Website = "updated-provider.com";
                state.AddressLine1 = "Updated line 1";
                state.AddressLine2 = "Updated line 2";
                state.Town = "Updated town";
                state.County = "Updated county";
                state.Postcode = "UP1 D8D";
                state.Latitude = 42;
                state.Longitude = 42;
            });

            var requestContent = new FormUrlEncodedContentBuilder()
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be($"/venues?providerId={providerId}");

            CosmosDbQueryDispatcher.VerifyExecuteQuery<UpdateVenue, OneOf<NotFound, Venue>>(q =>
                q.VenueId == venueId &&
                q.Name == "Updated name" &&
                q.Email == "updated@provider.com" &&
                q.PhoneNumber == "02345 678901" &&
                q.Website == "updated-provider.com" &&
                q.AddressLine1 == "Updated line 1" &&
                q.AddressLine2 == "Updated line 2" &&
                q.Town == "Updated town" &&
                q.County == "Updated county" &&
                q.Postcode == "UP1 D8D" &&
                q.Latitude == 42 &&
                q.Longitude == 42 &&
                q.UpdatedBy.UserId == User.UserId &&
                q.UpdatedDate == Clock.UtcNow);
        }

        private async Task<JourneyInstance<EditVenueJourneyModel>> CreateJourneyInstance(Guid venueId)
        {
            var state = await Factory.Services.GetRequiredService<EditVenueJourneyModelFactory>()
                .CreateModel(venueId);

            return CreateJourneyInstance(
                journeyName: "EditVenue",
                configureKeys: keysBuilder => keysBuilder.With("venueId", venueId),
                state);
        }
    }
}
