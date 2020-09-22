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
    public class WebsiteTests : MvcTestBase
    {
        public WebsiteTests(CourseDirectoryApplicationFactory factory)
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
            var venueId = await TestData.CreateVenue(providerId, website: "provider.com");

            var anotherProviderId = await TestData.CreateProvider(ukprn: 67890);

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}/website");

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

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}/website");

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
            var venueId = await TestData.CreateVenue(providerId, website: "provider.com");

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}/website");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("provider.com", doc.GetElementById("Website").GetAttribute("value"));
        }

        [Theory]
        [InlineData("new-provider.com")]
        [InlineData("")]
        public async Task Get_NewWebsiteAlreadySetInFormFlowInstance_RendersExpectedOutput(string existingValue)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = await TestData.CreateVenue(providerId, website: "provider.com");

            var formFlowInstance = await CreateFormFlowInstance(venueId);
            formFlowInstance.UpdateState(state => state.Website = existingValue);

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}/website");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal(existingValue, doc.GetElementById("Website").GetAttribute("value"));
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
                .Add("Website", "new-provider.com")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/website")
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
                .Add("Website", "new-provider.com")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/website")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Post_InvalidWebsite_RendersError()
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = await TestData.CreateVenue(providerId);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Website", ":bad/website")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/website")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("Website", "The format of URL is incorrect");
        }

        [Theory]
        [InlineData("new-provider.com")]
        [InlineData("http://new-provider.com")]
        [InlineData("https://new-provider.com")]
        [InlineData("")]
        public async Task Post_ValidRequest_UpdatesFormFlowInstanceAndRedirects(string website)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = await TestData.CreateVenue(providerId);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Website", website)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/website")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal($"/venues/{venueId}", response.Headers.Location.OriginalString);

            var formFlowInstance = GetFormFlowInstance(venueId);
            Assert.Equal(website, formFlowInstance.State.Website);
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
