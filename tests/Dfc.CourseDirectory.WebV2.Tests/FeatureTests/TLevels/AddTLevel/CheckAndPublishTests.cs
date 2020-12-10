using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.TLevels.AddTLevel;
using FluentAssertions;
using FluentAssertions.Execution;
using OneOf;
using OneOf.Types;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.TLevels.AddTLevel
{
    public class CheckAndPublishTests : AddTLevelTestBase
    {
        public CheckAndPublishTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(ProviderType.None)]
        [InlineData(ProviderType.Apprenticeships)]
        [InlineData(ProviderType.FE)]
        [InlineData(ProviderType.FE | ProviderType.Apprenticeships)]
        public async Task Get_ProviderIsNotTLevelsProvider_ReturnsForbidden(ProviderType providerType)
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: providerType);

            var venueId = await TestData.CreateVenue(providerId);

            var selectedTLevel = tLevelDefinitions.First();

            var journeyState = new AddTLevelJourneyModel();

            journeyState.SetTLevel(
                selectedTLevel.TLevelDefinitionId,
                selectedTLevel.Name);

            journeyState.SetDescription(
                "Who for",
                "Entry requirements",
                "What you'll learn",
                "How you'll learn",
                "How you'll be assessed",
                "What you can do next",
                isComplete: true);

            journeyState.SetDetails(
                "YOUR-REF",
                startDate: new DateTime(2021, 4, 1),
                locationVenueIds: new[] { venueId },
                website: "http://example.com/tlevel",
                isComplete: true);

            var journeyInstance = CreateJourneyInstance(providerId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/add/check-publish?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public Task Get_JourneyIsCompleted_ReturnsConflict() => JourneyIsCompletedReturnsConflict(info =>
        {
            var (providerId, _, journeyInstanceId) = info;

            return new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/add/check-publish?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}");
        });

        [Fact]
        public async Task Get_JourneyStateIsNotValid_ReturnsBadRequest()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var journeyState = new AddTLevelJourneyModel();

            var journeyInstance = CreateJourneyInstance(providerId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            journeyInstance.State.ValidStages.Should().Be(AddTLevelJourneyCompletedStages.None);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/add/check-publish?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_ValidRequest_ReturnsExpectedContent()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venueName = "T Level test venue";
            var venueId = await TestData.CreateVenue(providerId, venueName: venueName);

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

            journeyState.SetDetails(
                yourReference,
                startDate,
                locationVenueIds: new[] { venueId },
                website,
                isComplete: true);

            var journeyInstance = CreateJourneyInstance(providerId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/add/check-publish?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();

            using (new AssertionScope())
            {
                doc.GetSummaryListValueWithKey("Your reference").Should().Be(yourReference);
                doc.GetSummaryListValueWithKey("Start date").Should().Be($"{startDate:d MMMM yyyy}");
                doc.GetSummaryListValueWithKey("T Level location").Should().Be(venueName);
                doc.GetSummaryListValueWithKey("T Level webpage").Should().Be(website);
                doc.GetSummaryListValueWithKey("Who this T Level is for").Should().Be(whoFor);
                doc.GetSummaryListValueWithKey("Entry requirements").Should().Be(entryRequirements);
                doc.GetSummaryListValueWithKey("What you'll learn").Should().Be(whatYoullLearn);
                doc.GetSummaryListValueWithKey("How you'll learn").Should().Be(howYoullLearn);
                doc.GetSummaryListValueWithKey("How you'll be assessed").Should().Be(howYoullBeAssessed);
                doc.GetSummaryListValueWithKey("What you can do next").Should().Be(whatYouCanDoNext);
            }
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

            journeyState.SetDetails(
                yourReference,
                startDate,
                locationVenueIds: new[] { venueId },
                website,
                isComplete: true);

            var journeyInstance = CreateJourneyInstance(providerId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add/check-publish?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_JourneyStateIsNotValid_ReturnsBadRequest()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var journeyState = new AddTLevelJourneyModel();

            var journeyInstance = CreateJourneyInstance(providerId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            journeyInstance.State.ValidStages.Should().Be(AddTLevelJourneyCompletedStages.None);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add/check-publish?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public Task Post_JourneyIsCompleted_ReturnsConflict() => JourneyIsCompletedReturnsConflict(info =>
        {
            var (providerId, _, journeyInstanceId) = info;

            return new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add/check-publish?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder().ToContent()
            };
        });

        [Fact]
        public async Task Post_TLevelAlreadyExistsForStartDate_RendersError()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venueId = await TestData.CreateVenue(providerId);

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

            await TestData.CreateTLevel(
                providerId,
                selectedTLevel.TLevelDefinitionId,
                whoFor,
                entryRequirements,
                whatYoullLearn,
                howYoullLearn,
                howYoullBeAssessed,
                whatYouCanDoNext,
                yourReference,
                startDate,
                new[] { venueId },
                website,
                createdBy: User.ToUserInfo());

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

            journeyState.SetDetails(
                yourReference,
                startDate,
                locationVenueIds: new[] { venueId },
                website,
                isComplete: true);

            var journeyInstance = CreateJourneyInstance(providerId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            Guid createdTLevelId = default;
            SqlQuerySpy.Callback<CreateTLevel, OneOf<CreateTLevelFailedReason, Success>>(
                q => createdTLevelId = q.TLevelId);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add/check-publish?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("duplicate-date-error").Should().NotBeNull();
        }

        [Fact]
        public async Task Post_CreatesTLevelUpdatesJourneyStateAndRedirects()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venueId = await TestData.CreateVenue(providerId);

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

            journeyState.SetDetails(
                yourReference,
                startDate,
                locationVenueIds: new[] { venueId },
                website,
                isComplete: true);

            var journeyInstance = CreateJourneyInstance(providerId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            Guid createdTLevelId = default;
            SqlQuerySpy.Callback<CreateTLevel, OneOf<CreateTLevelFailedReason, Success>>(
                q => createdTLevelId = q.TLevelId);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add/check-publish?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString
                .Should().Be($"/t-levels/add/success?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}");

            SqlQuerySpy.VerifyQuery<CreateTLevel, OneOf<CreateTLevelFailedReason, Success>>(q =>
                q.CreatedBy.UserId == User.UserId &&
                q.CreatedOn == Clock.UtcNow &&
                q.EntryRequirements == entryRequirements &&
                q.HowYoullBeAssessed == howYoullBeAssessed &&
                q.HowYoullLearn == howYoullLearn &&
                q.LocationVenueIds.Single() == venueId &&
                q.ProviderId == providerId &&
                q.StartDate == startDate &&
                q.TLevelDefinitionId == selectedTLevel.TLevelDefinitionId &&
                q.Website == website &&
                q.WhatYouCanDoNext == whatYouCanDoNext &&
                q.WhatYoullLearn == q.WhatYoullLearn &&
                q.WhoFor == whoFor &&
                q.YourReference == q.YourReference);

            GetJourneyInstance<AddTLevelJourneyModel>(journeyInstanceId)
                .State.TLevelId.Should().Be(createdTLevelId);
        }
    }
}
