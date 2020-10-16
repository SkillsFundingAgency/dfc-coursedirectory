using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.EditVenue;
using FluentAssertions;
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
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementById("Name").InnerHtml.Should().Be("Test Venue");
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
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementById("Name").InnerHtml.Should().Be(existingValue);
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
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
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
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

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
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

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
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

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
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be($"/venues/{venueId}");

            var formFlowInstance = GetFormFlowInstance(venueId);
            formFlowInstance.State.Name.Should().Be("Another Venue");
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
