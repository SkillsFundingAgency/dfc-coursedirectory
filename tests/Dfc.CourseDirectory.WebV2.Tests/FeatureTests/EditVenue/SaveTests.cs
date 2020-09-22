using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.EditVenue;
using FormFlow;
using Microsoft.Extensions.DependencyInjection;
using OneOf;
using OneOf.Types;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.EditVenue
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
            var venueId = await TestData.CreateVenue(providerId, email: "person@provider.com");

            var anotherProviderId = await TestData.CreateProvider(ukprn: 67890);

            var formFlowInstance = await CreateFormFlowInstance(venueId);
            formFlowInstance.UpdateState(state =>
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
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Post_ValidRequest_UpdatesDatabaseAndRedirects()
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = await TestData.CreateVenue(providerId);

            var formFlowInstance = await CreateFormFlowInstance(venueId);
            formFlowInstance.UpdateState(state =>
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
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/Venues", response.Headers.Location.OriginalString);

            CosmosDbQueryDispatcher.VerifyExecuteQuery<UpdateVenue, OneOf<NotFound, Success>>(q =>
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
                q.Longitude == 42);
        }

        private async Task<FormFlowInstance<EditVenueFlowModel>> CreateFormFlowInstance(Guid venueId)
        {
            var state = await Factory.Services.GetRequiredService<EditVenueFlowModelFactory>()
                .CreateModel(venueId);

            return CreateFormFlowInstanceForRouteParameters(
                key: "EditVenue",
                routeParameters: new Dictionary<string, object>()
                {
                    { "venueId", venueId }
                },
                state);
        }
    }
}
