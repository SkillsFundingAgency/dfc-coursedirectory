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
    public class EmailTests : MvcTestBase
    {
        public EmailTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId, email: "person@provider.com")).Id;

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}/email");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementById("Email").GetAttribute("value").Should().Be("person@provider.com");
        }

        [Theory]
        [InlineData("another_person@provider.com")]
        [InlineData("")]
        public async Task Get_NewEmailAlreadySetInJourneyInstance_RendersExpectedOutput(string existingValue)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId, email: "person@provider.com")).Id;

            var journeyInstance = await CreateJourneyInstance(venueId);
            journeyInstance.UpdateState(state => state.Email = existingValue);

            var request = new HttpRequestMessage(HttpMethod.Get, $"venues/{venueId}/email");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementById("Email").GetAttribute("value").Should().Be(existingValue);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Post_UserCannotAccessVenue_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

            var anotherProvider = await TestData.CreateProvider();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Email", "user@provider.com")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/email")
            {
                Content = requestContent
            };

            await User.AsTestUser(userType, anotherProvider.ProviderId);

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
                .Add("Email", "user@provider.com")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/email")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_InvalidEmail_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Email", "bademail")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/email")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasErrorWithCode("Email", "VENUE_EMAIL_FORMAT");
        }

        [Theory]
        [InlineData("user@provider.com")]
        [InlineData("")]
        public async Task Post_ValidRequest_UpdatesJourneyInstanceAndRedirects(string email)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Email", email)
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/email")
            {
                Content = requestContent
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be($"/venues/{venueId}");

            var journeyInstance = GetJourneyInstance(venueId);
            journeyInstance.State.Email.Should().Be(email);
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
