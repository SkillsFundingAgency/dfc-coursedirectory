using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.TLevels.ViewAndEditTLevel
{
    public class ViewTLevelTests : MvcTestBase
    {
        public ViewTLevelTests(CourseDirectoryApplicationFactory factory)
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
            var anotherVenue = await TestData.CreateVenue(providerId, venueName: "Another T Level venue");

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

            var request = new HttpRequestMessage(HttpMethod.Get, $"/t-levels/{tLevel.TLevelId}");

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

            var request = new HttpRequestMessage(HttpMethod.Get, $"/t-levels/{tLevelId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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

            var venue = await TestData.CreateVenue(providerId, venueName: "T Level venue");
            var anotherVenue = await TestData.CreateVenue(providerId, venueName: "Another T Level venue");

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

            var request = new HttpRequestMessage(HttpMethod.Get, $"/t-levels/{tLevel.TLevelId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();

            using (new AssertionScope())
            {
                doc.GetSummaryListValueWithKey("T Level added on").Should().Be(tLevel.CreatedOn.ToString("d MMMM yyyy"));
                doc.GetSummaryListValueWithKey("Last updated").Should().Be(tLevel.UpdatedOn.ToString("d MMMM yyyy"));
                doc.GetSummaryListValueWithKey("Your reference").Should().Be(tLevel.YourReference);
                doc.GetSummaryListValueWithKey("Start date").Should().Be(tLevel.StartDate.ToString("d MMMM yyyy"));
                doc.GetAllElementsByTestId("LocationName").Select(l => l.TextContent.Trim()).Should().Equal(tLevel.Locations.Select(ll => ll.VenueName));
                doc.GetSummaryListValueWithKey("T Level webpage").Should().Be(tLevel.Website);
                doc.GetSummaryListValueWithKey("Who this T Level is for").Should().Be(tLevel.WhoFor);
                doc.GetSummaryListValueWithKey("Entry requirements").Should().Be(tLevel.EntryRequirements);
                doc.GetSummaryListValueWithKey("What you'll learn").Should().Be(tLevel.WhatYoullLearn);
                doc.GetSummaryListValueWithKey("How you'll learn").Should().Be(tLevel.HowYoullLearn);
                doc.GetSummaryListValueWithKey("How you'll be assessed").Should().Be(tLevel.HowYoullBeAssessed);
                doc.GetSummaryListValueWithKey("What you can do next").Should().Be(tLevel.WhatYouCanDoNext);
            }
        }
    }
}
