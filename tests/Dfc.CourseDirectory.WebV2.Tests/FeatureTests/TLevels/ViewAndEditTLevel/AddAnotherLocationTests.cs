using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.TLevels.ViewAndEditTLevel;
using FluentAssertions;
using FluentAssertions.Execution;
using FormFlow;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.TLevels.ViewAndEditTLevel
{
    public class AddAnotherLocationTests : MvcTestBase
    {
        public AddAnotherLocationTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Post_UserCannotAccessTLevel_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var anotherProviderId = await TestData.CreateProvider(ukprn: 23456, providerType: ProviderType.TLevels);

            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venue = await TestData.CreateVenue(providerId, venueName: "T Level venue");
            var venue2 = await TestData.CreateVenue(providerId, venueName: "Another T Level venue");

            var tLevelDefinition = tLevelDefinitions.First();
            var initialWhoFor = "Who for";
            var initialEntryRequirements = "Entry requirements";
            var initialWhatYoullLearn = "What you'll learn";
            var initialHowYoullLearn = "How you'll learn";
            var initialHowYoullBeAssessed = "How you'll be assessed";
            var initialWhatYouCanDoNext = "What you can do next";
            var initialYourReference = "YOUR-REF";
            var initialStartDate = new DateTime(2021, 4, 1);
            var initialWebsite = "http://example.com/tlevel";

            var tLevel = await TestData.CreateTLevel(
                providerId,
                tLevelDefinition.TLevelDefinitionId,
                whoFor: initialWhoFor,
                entryRequirements: initialEntryRequirements,
                whatYoullLearn: initialWhatYoullLearn,
                howYoullLearn: initialHowYoullLearn,
                howYoullBeAssessed: initialHowYoullBeAssessed,
                whatYouCanDoNext: initialWhatYouCanDoNext,
                yourReference: initialYourReference,
                startDate: initialStartDate,
                locationVenueIds: new[] { venue.Id },
                website: initialWebsite,
                createdBy: User.ToUserInfo());

            var journeyInstance = await CreateJourneyInstance(tLevel.TLevelId);
            var journeyInstanceId = journeyInstance.InstanceId;

            var updatedWhoFor = "Updated who for";
            var updatedEntryRequirements = "Updated entry requirements";
            var updatedWhatYoullLearn = "Updated what you'll learn";
            var updatedHowYoullLearn = "Updated how you'll learn";
            var updatedHowYoullBeAssessed = "Updated how you'll be assessed";
            var updatedWhatYouCanDoNext = "Updated what you can do next";
            var updatedYourReference = "UPDATED-YOUR-REF";
            var updatedStartDate = new DateTime(2022, 10, 2);
            var updatedWebsite = "http://example.com/tlevel2";

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/{tLevel.TLevelId}/add-location?ffiid={journeyInstance.InstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("YourReference", updatedYourReference)
                    .Add("StartDate.Day", updatedStartDate.Day)
                    .Add("StartDate.Month", updatedStartDate.Month)
                    .Add("StartDate.Year", updatedStartDate.Year)
                    .Add("LocationVenueIds", venue2.Id)
                    .Add("Website", updatedWebsite)
                    .Add("WhoFor", updatedWhoFor)
                    .Add("EntryRequirements", updatedEntryRequirements)
                    .Add("WhatYoullLearn", updatedWhatYoullLearn)
                    .Add("HowYoullLearn", updatedHowYoullLearn)
                    .Add("HowYoullBeAssessed", updatedHowYoullBeAssessed)
                    .Add("WhatYouCanDoNext", updatedWhatYouCanDoNext)
                    .ToContent()
            };

            await User.AsTestUser(userType, anotherProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_TLevelDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var tLevelId = Guid.NewGuid();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/{tLevelId}/add-location")
            {
                Content = new FormUrlEncodedContentBuilder().ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_SavesDetailsMarksStateInvalidAndRedirects()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venue = await TestData.CreateVenue(providerId, venueName: "T Level venue");
            var venue2 = await TestData.CreateVenue(providerId, venueName: "Another T Level venue");

            var tLevelDefinition = tLevelDefinitions.First();
            var initialWhoFor = "Who for";
            var initialEntryRequirements = "Entry requirements";
            var initialWhatYoullLearn = "What you'll learn";
            var initialHowYoullLearn = "How you'll learn";
            var initialHowYoullBeAssessed = "How you'll be assessed";
            var initialWhatYouCanDoNext = "What you can do next";
            var initialYourReference = "YOUR-REF";
            var initialStartDate = new DateTime(2021, 4, 1);
            var initialWebsite = "http://example.com/tlevel";

            var tLevel = await TestData.CreateTLevel(
                providerId,
                tLevelDefinition.TLevelDefinitionId,
                whoFor: initialWhoFor,
                entryRequirements: initialEntryRequirements,
                whatYoullLearn: initialWhatYoullLearn,
                howYoullLearn: initialHowYoullLearn,
                howYoullBeAssessed: initialHowYoullBeAssessed,
                whatYouCanDoNext: initialWhatYouCanDoNext,
                yourReference: initialYourReference,
                startDate: initialStartDate,
                locationVenueIds: new[] { venue.Id },
                website: initialWebsite,
                createdBy: User.ToUserInfo());

            var journeyInstance = await CreateJourneyInstance(tLevel.TLevelId);
            var journeyInstanceId = journeyInstance.InstanceId;

            var updatedWhoFor = "Updated who for";
            var updatedEntryRequirements = "Updated entry requirements";
            var updatedWhatYoullLearn = "Updated what you'll learn";
            var updatedHowYoullLearn = "Updated how you'll learn";
            var updatedHowYoullBeAssessed = "Updated how you'll be assessed";
            var updatedWhatYouCanDoNext = "Updated what you can do next";
            var updatedYourReference = "UPDATED-YOUR-REF";
            var updatedStartDate = new DateTime(2022, 10, 2);
            var updatedWebsite = "http://example.com/tlevel2";

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/{tLevel.TLevelId}/add-location?ffiid={journeyInstance.InstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("YourReference", updatedYourReference)
                    .Add("StartDate.Day", updatedStartDate.Day)
                    .Add("StartDate.Month", updatedStartDate.Month)
                    .Add("StartDate.Year", updatedStartDate.Year)
                    .Add("LocationVenueIds", venue2.Id)
                    .Add("Website", updatedWebsite)
                    .Add("WhoFor", updatedWhoFor)
                    .Add("EntryRequirements", updatedEntryRequirements)
                    .Add("WhatYoullLearn", updatedWhatYoullLearn)
                    .Add("HowYoullLearn", updatedHowYoullLearn)
                    .Add("HowYoullBeAssessed", updatedHowYoullBeAssessed)
                    .Add("WhatYouCanDoNext", updatedWhatYouCanDoNext)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().StartWith($"/venues/add");

            var journeyState = GetJourneyInstance<EditTLevelJourneyModel>(journeyInstanceId).State;

            using (new AssertionScope())
            {
                journeyState.YourReference.Should().Be(updatedYourReference);
                journeyState.StartDate.Should().Be(updatedStartDate);
                journeyState.LocationVenueIds.Should().OnlyContain(id => id == venue2.Id);
                journeyState.Website.Should().Be(updatedWebsite);
                journeyState.WhoFor.Should().Be(updatedWhoFor);
                journeyState.EntryRequirements.Should().Be(updatedEntryRequirements);
                journeyState.WhatYoullLearn.Should().Be(updatedWhatYoullLearn);
                journeyState.HowYoullLearn.Should().Be(updatedHowYoullLearn);
                journeyState.HowYoullBeAssessed.Should().Be(updatedHowYoullBeAssessed);
                journeyState.WhatYouCanDoNext.Should().Be(updatedWhatYouCanDoNext);
                journeyState.IsValid.Should().BeFalse();
            }
        }

        private async Task<JourneyInstance<EditTLevelJourneyModel>> CreateJourneyInstance(Guid tLevelId)
        {
            var state = await WithSqlQueryDispatcher(async dispatcher =>
            {
                var modelFactory = CreateInstance<EditTLevelJourneyModelFactory>(dispatcher);
                return await modelFactory.CreateModel(tLevelId);
            });

            var uniqueKey = Guid.NewGuid().ToString();

            return CreateJourneyInstance(
                "EditTLevel",
                keys => keys.With("tLevelId", tLevelId).Build(),
                state,
                uniqueKey: uniqueKey);
        }
    }
}
