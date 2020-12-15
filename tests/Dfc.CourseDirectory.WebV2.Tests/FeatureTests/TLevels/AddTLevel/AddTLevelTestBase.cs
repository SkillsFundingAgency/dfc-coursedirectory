using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Features.TLevels.AddTLevel;
using FluentAssertions;
using FormFlow;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.TLevels.AddTLevel
{
    public abstract class AddTLevelTestBase : MvcTestBase
    {
        protected AddTLevelTestBase(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        protected async Task JourneyIsCompletedReturnsConflict(
            Func<(Guid ProviderId, TLevel CreatedTLevel, JourneyInstanceId JourneyInstanceId), HttpRequestMessage> createRequest)
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venueId = (await TestData.CreateVenue(providerId)).Id;

            var tLevelDefinitionId = tLevelDefinitions.First().TLevelDefinitionId;

            var createdTLevel = await TestData.CreateTLevel(
                providerId,
                tLevelDefinitionId: tLevelDefinitionId,
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

            var request = createRequest((providerId, createdTLevel, journeyInstanceId));

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        protected JourneyInstance<AddTLevelJourneyModel> CreateJourneyInstance(
            Guid providerId,
            AddTLevelJourneyModel state = null)
        {
            state ??= new AddTLevelJourneyModel();
            var uniqueKey = Guid.NewGuid().ToString();

            return CreateJourneyInstance(
                journeyName: "AddTLevel",
                configureKeys: keysBuilder => keysBuilder.With("providerId", providerId),
                state,
                uniqueKey: uniqueKey);
        }
    }
}
