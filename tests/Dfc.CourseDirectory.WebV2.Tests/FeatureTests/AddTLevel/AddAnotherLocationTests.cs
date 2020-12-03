using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.AddTLevel;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.AddTLevel
{
    public class AddAnotherLocationTests : AddTLevelTestBase
    {
        public AddAnotherLocationTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(ProviderType.None)]
        [InlineData(ProviderType.Apprenticeships)]
        [InlineData(ProviderType.FE)]
        [InlineData(ProviderType.FE | ProviderType.Apprenticeships)]
        public async Task Post_ProviderIsNotTLevelsProvider_ReturnsForbidden(ProviderType providerType)
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(providerType: providerType);

            var venueId = await TestData.CreateVenue(providerId);
            var anotherVenueId = await TestData.CreateVenue(providerId, venueName: "Second Venue");

            var selectedTLevel = tLevelDefinitions.First();
            var whoFor = "Who for";
            var entryRequirements = "Entry requirements";
            var whatYoullLearn = "What you'll learn";
            var howYoullLearn = "How you'll learn";
            var howYoullBeAssessed = "How you'll be assessed";
            var whatYouCanDoNext = "What you can do next";
            var yourReference = "YOUR-REF";
            var startDate = new DateTime(2021, 4, 1);
            var website = "http://example.com/tlevel";

            var journeyState = new AddTLevelJourneyModel();

            journeyState.SetTLevel(
                selectedTLevel.TLevelDefinitionId,
                selectedTLevel.Name);

            journeyState.SetDescription(
                whoFor,
                entryRequirements,
                whatYoullLearn,
                howYoullLearn,
                howYoullBeAssessed,
                whatYouCanDoNext,
                isComplete: true);

            var journeyInstance = CreateJourneyInstance(providerId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add/add-location?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("YourReference", yourReference)
                    .Add("StartDate.Day", startDate.Day)
                    .Add("StartDate.Month", startDate.Month)
                    .Add("StartDate.Year", startDate.Year)
                    .Add("LocationVenueIds", venueId)
                    .Add("Website", website)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public Task Post_JourneyIsCompleted_ReturnsConflict() => JourneyIsCompletedReturnsConflict(info =>
        {
            var (providerId, createdTLevel, journeyInstanceId) = info;

            var yourReference = "YOUR-REF";
            var startDate = new DateTime(2021, 4, 1);
            var website = "http://example.com/tlevel";

            return new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add/add-location?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("YourReference", yourReference)
                    .Add("StartDate.Day", startDate.Day)
                    .Add("StartDate.Month", startDate.Month)
                    .Add("StartDate.Year", startDate.Year)
                    .Add("LocationVenueIds", createdTLevel.Locations[0])
                    .Add("Website", website)
                    .ToContent()
            };
        });

        [Fact]
        public async Task Post_JourneyStateIsNotValid_ReturnsBadRequest()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venueId = await TestData.CreateVenue(providerId);

            var yourReference = "YOUR-REF";
            var startDate = new DateTime(2021, 4, 1);
            var website = "http://example.com/tlevel";

            var journeyState = new AddTLevelJourneyModel();

            journeyState.ValidStages.Should().Be(AddTLevelJourneyCompletedStages.None);

            var journeyInstance = CreateJourneyInstance(providerId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add/add-location?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("YourReference", yourReference)
                    .Add("StartDate.Day", startDate.Day)
                    .Add("StartDate.Month", startDate.Month)
                    .Add("StartDate.Year", startDate.Year)
                    .Add("LocationVenueIds", venueId)
                    .Add("Website", website)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_SavesDetailsMarksStageNotCompletedAndRedirects()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venueId = await TestData.CreateVenue(providerId);
            var anotherVenueId = await TestData.CreateVenue(providerId, venueName: "Second Venue");

            var selectedTLevel = tLevelDefinitions.First();
            var whoFor = "Who for";
            var entryRequirements = "Entry requirements";
            var whatYoullLearn = "What you'll learn";
            var howYoullLearn = "How you'll learn";
            var howYoullBeAssessed = "How you'll be assessed";
            var whatYouCanDoNext = "What you can do next";
            var yourReference = "YOUR-REF";
            var startDate = new DateTime(2021, 4, 1);
            var website = "http://example.com/tlevel";

            var journeyState = new AddTLevelJourneyModel();

            journeyState.SetTLevel(
                selectedTLevel.TLevelDefinitionId,
                selectedTLevel.Name);

            journeyState.SetDescription(
                whoFor,
                entryRequirements,
                whatYoullLearn,
                howYoullLearn,
                howYoullBeAssessed,
                whatYouCanDoNext,
                isComplete: true);

            var journeyInstance = CreateJourneyInstance(providerId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add/add-location?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("YourReference", yourReference)
                    .Add("StartDate.Day", startDate.Day)
                    .Add("StartDate.Month", startDate.Month)
                    .Add("StartDate.Year", startDate.Year)
                    .Add("LocationVenueIds", venueId)
                    .Add("Website", website)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().StartWith($"/Venues/Add");

            journeyState = GetJourneyInstance<AddTLevelJourneyModel>(journeyInstanceId).State;

            using (new AssertionScope())
            {
                journeyState.YourReference.Should().Be(yourReference);
                journeyState.StartDate.Should().Be(startDate);
                journeyState.LocationVenueIds.Should().BeEquivalentTo(new[] { venueId });
                journeyState.Website.Should().Be(website);

                journeyState.ValidStages.Should().Be(
                    AddTLevelJourneyCompletedStages.SelectTLevel |
                    AddTLevelJourneyCompletedStages.Description);
            }
        }
    }
}
