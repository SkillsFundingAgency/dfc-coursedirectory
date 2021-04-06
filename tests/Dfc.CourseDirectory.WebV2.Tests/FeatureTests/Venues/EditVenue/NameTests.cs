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
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId, venueName: "Test Venue")).Id;

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
        public async Task Get_NewNameAlreadySetInJourneyInstance_RendersExpectedOutput(string existingValue)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId, venueName: "Test Venue")).Id;

            var journeyInstance = await CreateJourneyInstance(venueId);
            journeyInstance.UpdateState(state => state.Name = existingValue);

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
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

            var anotherProvider = await TestData.CreateProvider();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Name", "Another Venue")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"venues/{venueId}/name")
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
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

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
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

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
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

            await TestData.CreateVenue(provider.ProviderId, venueName: "Venue B");

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
        public async Task Post_ValidRequest_UpdatesJourneyInstanceAndRedirects()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

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

            var journeyInstance = GetJourneyInstance(venueId);
            journeyInstance.State.Name.Should().Be("Another Venue");
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
