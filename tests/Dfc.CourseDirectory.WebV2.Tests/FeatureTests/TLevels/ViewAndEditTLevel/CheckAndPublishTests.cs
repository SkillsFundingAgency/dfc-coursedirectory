using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.TLevels.ViewAndEditTLevel;
using FluentAssertions;
using FluentAssertions.Execution;
using FormFlow;
using OneOf;
using OneOf.Types;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.TLevels.ViewAndEditTLevel
{
    public class CheckAndPublishTests : MvcTestBase
    {
        public CheckAndPublishTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_UserCannotAccessTLevel_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var anotherProviderId = await TestData.CreateProvider(ukprn: 23456, providerType: ProviderType.TLevels);

            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venue = await TestData.CreateVenue(providerId, venueName: "T Level venue");

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
                providerId,
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
                $"/t-levels/{tLevel.TLevelId}/check-publish?ffiid={journeyInstance.InstanceId.UniqueKey}");

            await User.AsTestUser(userType, anotherProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Get_TLevelDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var tLevelId = Guid.NewGuid();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/{tLevelId}/check-publish");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venue = await TestData.CreateVenue(providerId, venueName: "T Level venue");

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
                providerId,
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
                $"/t-levels/{tLevel.TLevelId}/check-publish?ffiid={journeyInstance.InstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();

            using (new AssertionScope())
            {
                doc.GetElementByTestId("TLevelName").TextContent.Trim().Should().Be(tLevelDefinition.Name);
                doc.GetSummaryListValueWithKey("Your reference").Should().Be(yourReference);
                doc.GetSummaryListValueWithKey("Start date").Should().Be($"{startDate:d MMMM yyyy}");
                doc.GetSummaryListValueWithKey("T Level location").Should().Be(venue.VenueName);
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
                providerId,
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
                HttpMethod.Post,
                $"/t-levels/{tLevel.TLevelId}/check-publish?ffiid={journeyInstance.InstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder().ToContent()
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
                $"/t-levels/{tLevelId}/check-publish")
            {
                Content = new FormUrlEncodedContentBuilder().ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_JourneyStateNotValid_ReturnsBadRequest()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venue = await TestData.CreateVenue(providerId, venueName: "T Level venue");

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
                providerId,
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
            journeyInstance.UpdateState(state => state.IsValid = false);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/{tLevel.TLevelId}/check-publish?ffiid={journeyInstance.InstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder().ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_TLevelAlreadyExistsForStartDate_RendersError()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venue = await TestData.CreateVenue(providerId, venueName: "T Level venue");

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
                providerId,
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

            var updatedStartDate = new DateTime(2022, 10, 10);
            journeyInstance.UpdateState(state => state.StartDate = updatedStartDate);

            var anotherTLevel = await TestData.CreateTLevel(
                providerId,
                tLevelDefinition.TLevelDefinitionId,
                whoFor: whoFor,
                entryRequirements: entryRequirements,
                whatYoullLearn: whatYoullLearn,
                howYoullLearn: howYoullLearn,
                howYoullBeAssessed: howYoullBeAssessed,
                whatYouCanDoNext: whatYouCanDoNext,
                yourReference: yourReference,
                startDate: updatedStartDate,
                locationVenueIds: new[] { venue.Id },
                website: website,
                createdBy: User.ToUserInfo());

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/{tLevel.TLevelId}/check-publish?ffiid={journeyInstance.InstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder().ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.GetElementByTestId("duplicate-date-error").Should().NotBeNull();
        }

        [Fact]
        public async Task Post_ValidRequest_UpdatesTLevelAndRedirects()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venue = await TestData.CreateVenue(providerId, venueName: "T Level venue");

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
                providerId,
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
                HttpMethod.Post,
                $"/t-levels/{tLevel.TLevelId}/check-publish?ffiid={journeyInstance.InstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder().ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString
                .Should().Be($"/t-levels/{tLevel.TLevelId}/success?ffiid={journeyInstance.InstanceId.UniqueKey}");

            SqlQuerySpy.VerifyQuery<UpdateTLevel, OneOf<NotFound, UpdateTLevelFailedReason, Success>>(q =>
                q.UpdatedBy.UserId == User.UserId &&
                q.UpdatedOn == Clock.UtcNow &&
                q.EntryRequirements == entryRequirements &&
                q.HowYoullBeAssessed == howYoullBeAssessed &&
                q.HowYoullLearn == howYoullLearn &&
                q.LocationVenueIds.Single() == venue.Id &&
                q.StartDate == startDate &&
                q.Website == website &&
                q.WhatYouCanDoNext == whatYouCanDoNext &&
                q.WhatYoullLearn == q.WhatYoullLearn &&
                q.WhoFor == whoFor &&
                q.YourReference == q.YourReference);
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
