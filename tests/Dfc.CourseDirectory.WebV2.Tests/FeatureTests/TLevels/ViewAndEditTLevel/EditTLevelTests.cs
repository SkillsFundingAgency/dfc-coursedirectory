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
    public class EditTLevelTests : MvcTestBase
    {
        public EditTLevelTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_UserCannotAccessTLevel_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var anotherProvider = await TestData.CreateProvider(providerType: ProviderType.TLevels);

            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venue = await TestData.CreateVenue(provider.ProviderId, venueName: "T Level venue");

            var tLevelDefinition = tLevelDefinitions.First();
            var whoFor = "Who for";
            var entryRequirements = "Entry requirements";
            var whatYoullLearn = "What you'll learn";
            var howYoullLearn = "How you'll learn";
            var howYoullBeAssessed = "How you'll be assessed";
            var whatYouCanDoNext = "What you can do next";
            var yourReference = "YOUR-REF";
            var startDate = new DateTime(2021, 4, 1);
            var website = "http://example.com/tlevel";

            var tLevel = await TestData.CreateTLevel(
                provider.ProviderId,
                tLevelDefinition.TLevelDefinitionId,
                whoFor: whoFor,
                entryRequirements: entryRequirements,
                whatYoullLearn: whatYoullLearn,
                howYoullLearn: howYoullLearn,
                howYoullBeAssessed: howYoullBeAssessed,
                whatYouCanDoNext: whatYouCanDoNext,
                yourReference: yourReference,
                startDate: startDate,
                locationVenueIds: new[] { venue.Id },
                website: website,
                createdBy: User.ToUserInfo());

            var journeyInstance = await CreateJourneyInstance(tLevel.TLevelId);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/{tLevel.TLevelId}/edit?ffiid={journeyInstance.InstanceId.UniqueKey}");

            await User.AsTestUser(userType, anotherProvider.ProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Get_TLevelDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.TLevels);

            var tLevelId = Guid.NewGuid();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/{tLevelId}/edit&providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_VenueIdPassedFromCreateVenueCallack_AddsVenueToLocationVenueIds()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venue = await TestData.CreateVenue(provider.ProviderId, venueName: "T Level venue");
            var anotherVenue = await TestData.CreateVenue(provider.ProviderId, venueName: "Another T Level venue");

            var tLevelDefinition = tLevelDefinitions.First();
            var whoFor = "Who for";
            var entryRequirements = "Entry requirements";
            var whatYoullLearn = "What you'll learn";
            var howYoullLearn = "How you'll learn";
            var howYoullBeAssessed = "How you'll be assessed";
            var whatYouCanDoNext = "What you can do next";
            var yourReference = "YOUR-REF";
            var startDate = new DateTime(2021, 4, 1);
            var website = "http://example.com/tlevel";

            var tLevel = await TestData.CreateTLevel(
                provider.ProviderId,
                tLevelDefinition.TLevelDefinitionId,
                whoFor: whoFor,
                entryRequirements: entryRequirements,
                whatYoullLearn: whatYoullLearn,
                howYoullLearn: howYoullLearn,
                howYoullBeAssessed: howYoullBeAssessed,
                whatYouCanDoNext: whatYouCanDoNext,
                yourReference: yourReference,
                startDate: startDate,
                locationVenueIds: new[] { venue.Id },
                website: website,
                createdBy: User.ToUserInfo());

            var journeyInstance = await CreateJourneyInstance(tLevel.TLevelId);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/{tLevel.TLevelId}/edit?ffiid={journeyInstance.InstanceId.UniqueKey}&venueId={anotherVenue.Id}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId($"LocationVenueIds-{anotherVenue.Id}").GetAttribute("checked").Should().Be("checked");
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venue = await TestData.CreateVenue(provider.ProviderId, venueName: "T Level venue");

            var tLevelDefinition = tLevelDefinitions.First();
            var whoFor = "Who for";
            var entryRequirements = "Entry requirements";
            var whatYoullLearn = "What you'll learn";
            var howYoullLearn = "How you'll learn";
            var howYoullBeAssessed = "How you'll be assessed";
            var whatYouCanDoNext = "What you can do next";
            var yourReference = "YOUR-REF";
            var startDate = new DateTime(2021, 4, 1);
            var website = "http://example.com/tlevel";

            var tLevel = await TestData.CreateTLevel(
                provider.ProviderId,
                tLevelDefinition.TLevelDefinitionId,
                whoFor: whoFor,
                entryRequirements: entryRequirements,
                whatYoullLearn: whatYoullLearn,
                howYoullLearn: howYoullLearn,
                howYoullBeAssessed: howYoullBeAssessed,
                whatYouCanDoNext: whatYouCanDoNext,
                yourReference: yourReference,
                startDate: startDate,
                locationVenueIds: new[] { venue.Id },
                website: website,
                createdBy: User.ToUserInfo());

            var journeyInstance = await CreateJourneyInstance(tLevel.TLevelId);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/{tLevel.TLevelId}/edit?ffiid={journeyInstance.InstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            var vs = doc.GetAllElementsByTestId("LocationVenueNames").Select(e => e.TextContent.Trim());

            using (new AssertionScope())
            {
                doc.GetElementByTestId("TLevelName").TextContent.Should().Be(tLevelDefinition.Name);
                doc.GetElementById("YourReference").GetAttribute("value").Should().Be(yourReference);
                doc.GetElementById("StartDate.Day").GetAttribute("value").Should().Be(startDate.Day.ToString());
                doc.GetElementById("StartDate.Month").GetAttribute("value").Should().Be(startDate.Month.ToString());
                doc.GetElementById("StartDate.Year").GetAttribute("value").Should().Be(startDate.Year.ToString());
                doc.GetElementByTestId($"LocationVenueIds-{venue.Id}").GetAttribute("checked").Should().Be("checked");
                doc.GetElementById("Website").GetAttribute("value").Should().Be(website);
                doc.GetElementById("WhoFor").TextContent.Should().Be(whoFor);
                doc.GetElementById("EntryRequirements").TextContent.Should().Be(entryRequirements);
                doc.GetElementById("WhatYoullLearn").TextContent.Should().Be(whatYoullLearn);
                doc.GetElementById("HowYoullLearn").TextContent.Should().Be(howYoullLearn);
                doc.GetElementById("HowYoullBeAssessed").TextContent.Should().Be(howYoullBeAssessed);
                doc.GetElementById("WhatYouCanDoNext").TextContent.Should().Be(whatYouCanDoNext);
            }
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Post_UserCannotAccessTLevel_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var anotherProvider = await TestData.CreateProvider(providerType: ProviderType.TLevels);

            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venue = await TestData.CreateVenue(provider.ProviderId, venueName: "T Level venue");
            var venue2 = await TestData.CreateVenue(provider.ProviderId, venueName: "Another T Level venue");

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
                provider.ProviderId,
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
            journeyInstance.Complete();
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
                $"/t-levels/{tLevel.TLevelId}/edit?ffiid={journeyInstance.InstanceId.UniqueKey}")
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

            await User.AsTestUser(userType, anotherProvider.ProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_TLevelDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venue2 = await TestData.CreateVenue(provider.ProviderId, venueName: "Another T Level venue");

            var tLevelId = Guid.NewGuid();

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
                $"/t-levels/{tLevelId}/edit")
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
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_ValidRequest_UpdatesJourneyStateAndRedirects()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venue = await TestData.CreateVenue(provider.ProviderId, venueName: "T Level venue");
            var venue2 = await TestData.CreateVenue(provider.ProviderId, venueName: "Another T Level venue");

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
                provider.ProviderId,
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
                $"/t-levels/{tLevel.TLevelId}/edit?ffiid={journeyInstance.InstanceId.UniqueKey}")
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
            response.Headers.Location.OriginalString
                .Should().Be($"/t-levels/{tLevel.TLevelId}/check-publish?ffiid={journeyInstanceId.UniqueKey}");

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
                journeyState.IsValid.Should().BeTrue();
            }
        }

        private async Task<JourneyInstance> CreateJourneyInstance(Guid tLevelId)
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
