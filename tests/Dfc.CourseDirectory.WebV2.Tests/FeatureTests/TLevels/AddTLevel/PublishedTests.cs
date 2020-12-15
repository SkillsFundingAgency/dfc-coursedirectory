using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Features.TLevels.AddTLevel;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.TLevels.AddTLevel
{
    public class PublishedTests : AddTLevelTestBase
    {
        public PublishedTests(CourseDirectoryApplicationFactory factory)
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

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var createdTLevel = await TestData.CreateTLevel(
                providerId,
                tLevelDefinitionId: tLevelDefinitions.First().TLevelDefinitionId,
                whoFor: "Who for",
                entryRequirements: "Entry requirements",
                whatYoullLearn: "What you'll learn",
                howYoullLearn: "How you'll learn",
                howYoullBeAssessed: "How you'll be assessed",
                whatYouCanDoNext: "What you can do next",
                yourReference: "YOUR-REF",
                startDate: new DateTime(2021, 4, 1),
                locationVenueIds: new[] { venueId },
                website: "http://example.com/tlevel",
                createdBy: User.ToUserInfo());

            var journeyState = new AddTLevelJourneyModel();

            journeyState.SetTLevel(
                createdTLevel.TLevelDefinition.TLevelDefinitionId,
                createdTLevel.TLevelDefinition.Name);

            journeyState.SetDescription(
                createdTLevel.WhoFor,
                createdTLevel.EntryRequirements,
                createdTLevel.WhatYoullLearn,
                createdTLevel.HowYoullLearn,
                createdTLevel.HowYoullBeAssessed,
                createdTLevel.WhatYouCanDoNext,
                isComplete: true);

            journeyState.SetDetails(
                createdTLevel.YourReference,
                createdTLevel.StartDate,
                createdTLevel.Locations.Select(l => l.VenueId),
                createdTLevel.Website,
                isComplete: true);

            journeyState.SetCreatedTLevel(createdTLevel.TLevelId);

            var journeyInstance = CreateJourneyInstance(providerId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            journeyInstance.Complete();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/add/success?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

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

            journeyInstance.Completed.Should().BeFalse();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/add/success?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_ValidRequest_ReturnsOk()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var createdTLevel = await TestData.CreateTLevel(
                providerId,
                tLevelDefinitionId: tLevelDefinitions.First().TLevelDefinitionId,
                whoFor: "Who for",
                entryRequirements: "Entry requirements",
                whatYoullLearn: "What you'll learn",
                howYoullLearn: "How you'll learn",
                howYoullBeAssessed: "How you'll be assessed",
                whatYouCanDoNext: "What you can do next",
                yourReference: "YOUR-REF",
                startDate: new DateTime(2021, 4, 1),
                locationVenueIds: new[] { venueId },
                website: "http://example.com/tlevel",
                createdBy: User.ToUserInfo());

            var journeyState = new AddTLevelJourneyModel();

            journeyState.SetTLevel(
                createdTLevel.TLevelDefinition.TLevelDefinitionId,
                createdTLevel.TLevelDefinition.Name);

            journeyState.SetDescription(
                createdTLevel.WhoFor,
                createdTLevel.EntryRequirements,
                createdTLevel.WhatYoullLearn,
                createdTLevel.HowYoullLearn,
                createdTLevel.HowYoullBeAssessed,
                createdTLevel.WhatYouCanDoNext,
                isComplete: true);

            journeyState.SetDetails(
                createdTLevel.YourReference,
                createdTLevel.StartDate,
                createdTLevel.Locations.Select(l => l.VenueId),
                createdTLevel.Website,
                isComplete: true);

            journeyState.SetCreatedTLevel(createdTLevel.TLevelId);

            var journeyInstance = CreateJourneyInstance(providerId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            journeyInstance.Complete();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/add/success?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
