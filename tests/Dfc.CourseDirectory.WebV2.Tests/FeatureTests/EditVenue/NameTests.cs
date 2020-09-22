using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.EditVenue;
using FormFlow;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.EditVenue
{
    public class NameTests : MvcTestBase
    {
        public NameTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_UserCannotAccessVenue_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(ukprn: 12345);
            var venueId = await TestData.CreateVenue(providerId);

            var anotherProviderId = await TestData.CreateProvider(ukprn: 67890);

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}/name");

            await User.AsTestUser(userType, anotherProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Get_VenueDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var venueId = Guid.NewGuid();

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}/name");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = await TestData.CreateVenue(providerId, venueName: "Test Venue");

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}/name");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Test Venue", doc.GetElementById("Name").InnerHtml);
        }

        [Theory]
        [InlineData("Second Venue")]
        public async Task Get_NewNameAlreadySetInFormFlowInstance_RendersExpectedOutput(string existingValue)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = await TestData.CreateVenue(providerId, venueName: "Test Venue");

            var formFlowInstance = await CreateFormFlowInstance(venueId);
            formFlowInstance.UpdateState(state => state.Name = existingValue);

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}/name");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal(existingValue, doc.GetElementById("Name").InnerHtml);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Post_UserCannotAccessVenue_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(ukprn: 12345);
            var venueId = await TestData.CreateVenue(providerId);

            var anotherProviderId = await TestData.CreateProvider(ukprn: 67890);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Name", "Another Venue")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/name")
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
                .Add("Name", "Another Venue")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/name")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Post_NameEmpty_RendersError()
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = await TestData.CreateVenue(providerId);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Name", "")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/name")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("Name", "Enter location name");
        }

        [Fact]
        public async Task Post_NameTooLong_RendersError()
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = await TestData.CreateVenue(providerId);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Name", new string('x', 251))  // limit is 250
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/name")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("Name", "Location name must be 250 characters or fewer");
        }

        [Fact]
        public async Task Post_DuplicateName_RendersError()
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = await TestData.CreateVenue(providerId);

            await TestData.CreateVenue(providerId, venueName: "Venue B");

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Name", "Venue B")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/name")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("Name", "Location name must not already exist");
        }

        [Fact]
        public async Task Post_ValidRequest_UpdatesFormFlowInstanceAndRedirects()
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = await TestData.CreateVenue(providerId);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Name", "Another Venue")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/name")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal($"/venues/{venueId}", response.Headers.Location.OriginalString);

            var formFlowInstance = GetFormFlowInstance(venueId);
            Assert.Equal("Another Venue", formFlowInstance.State.Name);
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

        private FormFlowInstance<EditVenueFlowModel> GetFormFlowInstance(Guid venueId) =>
            GetFormFlowInstanceForRouteParameters<EditVenueFlowModel>(
                key: "EditVenue",
                routeParameters: new Dictionary<string, object>()
                {
                    { "venueId", venueId }
                });
    }
}
