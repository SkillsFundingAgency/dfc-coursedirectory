﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.Venues.EditVenue;
using FluentAssertions;
using FormFlow;
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
            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: "Test Venue")).VenueId;

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
            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: "Test Venue")).VenueId;

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
            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

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
            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

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
            doc.AssertHasErrorWithCode("Name", "VENUE_NAME_REQUIRED");
        }

        [Fact]
        public async Task Post_NameTooLong_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

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
            doc.AssertHasErrorWithCode("Name", "VENUE_NAME_MAXLENGTH");
        }

        [Fact]
        public async Task Post_DuplicateName_RendersError()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

            await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: "Venue B");

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
            doc.AssertHasErrorWithCode("Name", "VENUE_NAME_UNIQUE");
        }

        [Fact]
        public async Task Post_ValidRequest_UpdatesJourneyInstanceAndRedirects()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

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
            var state = await WithSqlQueryDispatcher(async dispatcher =>
            {
                var modelFactory = CreateInstance<EditVenueJourneyModelFactory>(dispatcher);
                return await modelFactory.CreateModel(venueId);
            });

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
