using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.Venues.EditVenue;
using FluentAssertions;
using FormFlow;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Venues.EditVenue
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
            var venueId = (await TestData.CreateVenue(providerId, telephone: "020 7946 0000")).Id;

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}/phone-number");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementById("PhoneNumber").GetAttribute("value").Should().Be("020 7946 0000");
        }

        [Theory]
        [InlineData("05678 912345")]
        [InlineData("")]
        public async Task Get_NewPhoneNumberAlreadySetInJourneyInstance_RendersExpectedOutput(string existingValue)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(providerId, telephone: "020 7946 0000")).Id;

            var journeyInstance = await CreateJourneyInstance(venueId);
            journeyInstance.UpdateState(state => state.PhoneNumber = existingValue);

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
            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var anotherProviderId = await TestData.CreateProvider(ukprn: 67890);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("PhoneNumber", "020 7946 0000")
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
                .Add("PhoneNumber", "020 7946 0000")
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
            var venueId = (await TestData.CreateVenue(providerId)).Id;

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
        [InlineData("020 7946 0000")]
        [InlineData("")]
        public async Task Post_ValidRequest_UpdatesJourneyInstanceAndRedirects(string phoneNumber)
        {
            // Arrange
            var providerId = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(providerId)).Id;

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

            var journeyInstance = GetJourneyInstance(venueId);
            journeyInstance.State.PhoneNumber.Should().Be(phoneNumber);
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

        private JourneyInstance<EditVenueJourneyModel> GetJourneyInstance(Guid venueId) =>
            GetJourneyInstance<EditVenueJourneyModel>(
                journeyName: "EditVenue",
                configureKeys: keysBuilder => keysBuilder.With("venueId", venueId));
    }
}
