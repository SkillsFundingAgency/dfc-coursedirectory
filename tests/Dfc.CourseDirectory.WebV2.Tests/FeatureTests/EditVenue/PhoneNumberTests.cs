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
    public class PhoneNumberTests : MvcTestBase
    {
        public PhoneNumberTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = await TestData.CreateVenue(providerId, telephone: "01234 567890");

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}/phone-number");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementById("PhoneNumber").GetAttribute("value").Should().Be("01234 567890");
        }

        [Theory]
        [InlineData("05678 912345")]
        [InlineData("")]
        public async Task Get_NewPhoneNumberAlreadySetInFormFlowInstance_RendersExpectedOutput(string existingValue)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = await TestData.CreateVenue(providerId, telephone: "01234 567890");

            var formFlowInstance = await CreateFormFlowInstance(venueId);
            formFlowInstance.UpdateState(state => state.PhoneNumber = existingValue);

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}/phone-number");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementById("PhoneNumber").GetAttribute("value").Should().Be(existingValue);
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
                .Add("PhoneNumber", "01234 567890")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/phone-number")
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
                .Add("PhoneNumber", "01234 567890")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/phone-number")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_InvalidPhoneNumber_RendersError()
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = await TestData.CreateVenue(providerId);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("PhoneNumber", "xxx")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/phone-number")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError("PhoneNumber", "Enter a telephone number in the correct format");
        }

        [Theory]
        [InlineData("01234 567890")]
        [InlineData("")]
        public async Task Post_ValidRequest_UpdatesFormFlowInstanceAndRedirects(string phoneNumber)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = await TestData.CreateVenue(providerId);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("PhoneNumber", phoneNumber)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/phone-number")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be($"/venues/{venueId}");

            var formFlowInstance = GetFormFlowInstance(venueId);
            formFlowInstance.State.PhoneNumber.Should().Be(phoneNumber);
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
