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
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.TLevels.AddTLevel
{
    public class SelectTLevelTests : AddTLevelTestBase
    {
        public SelectTLevelTests(CourseDirectoryApplicationFactory factory)
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
            var providerId = await TestData.CreateProvider(providerType: providerType);

            var journeyInstance = CreateJourneyInstance(providerId);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/add?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}");

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
                $"/t-levels/add?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}");
        });

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var journeyInstance = CreateJourneyInstance(providerId);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/add?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();

            foreach (var authorizedTLevelDefinitionId in authorizedTLevelDefinitionIds)
            {
                doc.GetElementByTestId($"tlevel-{authorizedTLevelDefinitionId}").GetAttribute("checked")
                    .Should().NotBe("checked");
            }
        }

        [Fact]
        public async Task Get_TLevelAlreadySelected_HasSelectedOptionChecked()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var selectedTLevel = tLevelDefinitions.First();

            var journeyState = new AddTLevelJourneyModel();
            journeyState.SetTLevel(
                selectedTLevel.TLevelDefinitionId,
                selectedTLevel.Name);

            var journeyInstance = CreateJourneyInstance(providerId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/add?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();

            using (new AssertionScope())
            {
                foreach (var authorizedTLevelDefinitionId in authorizedTLevelDefinitionIds)
                {
                    var radioElementChecked = doc.GetElementByTestId($"tlevel-{authorizedTLevelDefinitionId}")
                        .GetAttribute("checked");

                    if (authorizedTLevelDefinitionId == selectedTLevel.TLevelDefinitionId)
                    {
                        radioElementChecked.Should().Be("checked");
                    }
                    else
                    {
                        radioElementChecked.Should().NotBe("checked");
                    }
                }
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

            var providerId = await TestData.CreateProvider(providerType: providerType);

            var selectedTLevelId = tLevelDefinitions.First().TLevelDefinitionId;

            var journeyInstance = CreateJourneyInstance(providerId);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("SelectedTLevelDefinitionId", selectedTLevelId)
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
            var (providerId, tLevel, journeyInstanceId) = info;

            return new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("SelectedTLevelDefinitionId", tLevel.TLevelDefinition.TLevelDefinitionId)
                    .ToContent()
            };
        });

        [Fact]
        public async Task Post_NoTLevelSelected_RendersError()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var selectedTLevelId = tLevelDefinitions.First().TLevelDefinitionId;

            var journeyInstance = CreateJourneyInstance(providerId);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError(
                "SelectedTLevelDefinitionId",
                "Select the T Level qualification to publish to the course directory");
        }

        [Fact]
        public async Task Post_InvalidTLevelSelected_RendersError()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var selectedTLevelId = Guid.NewGuid();

            var journeyInstance = CreateJourneyInstance(providerId);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("SelectedTLevelDefinitionId", selectedTLevelId)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError(
                "SelectedTLevelDefinitionId",
                "Select the T Level qualification to publish to the course directory");
        }

        [Fact]
        public async Task Post_UnauthorizedTLevelSelected_RendersError()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionId = tLevelDefinitions.First().TLevelDefinitionId;
            var selectedTLevelId = tLevelDefinitions.Last().TLevelDefinitionId;
            selectedTLevelId.Should().NotBe(authorizedTLevelDefinitionId);

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: new[] { authorizedTLevelDefinitionId });

            var journeyInstance = CreateJourneyInstance(providerId);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("SelectedTLevelDefinitionId", selectedTLevelId)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError(
                "SelectedTLevelDefinitionId",
                "Select the T Level qualification to publish to the course directory");
        }

        [Fact]
        public async Task Post_NoCurrentTLevelSelected_UpdatesJourneyStateAndRedirects()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var selectedTLevelId = tLevelDefinitions.First().TLevelDefinitionId;

            var journeyInstance = CreateJourneyInstance(providerId);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("SelectedTLevelDefinitionId", selectedTLevelId)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be(
                $"/t-levels/add/description?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}");

            var journeyState = GetJourneyInstance<AddTLevelJourneyModel>(journeyInstanceId).State;
            journeyState.TLevelDefinitionId.Should().Be(selectedTLevelId);
            journeyState.CompletedStages.Should().Be(AddTLevelJourneyCompletedStages.SelectTLevel);
        }

        [Fact]
        public async Task Post_SelectedTLevelIsDifferentToCurrent_OverwritesDescriptionWithExemplarAndResetsJourneyStage()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var currentlySelectedTLevel = tLevelDefinitions.First();

            var currentWhoFor = "Who for";
            var currentEntryRequirements = "Entry requirements";
            var currentWhatYoullLearn = "What you'll learn";
            var currentHowYoullLearn = "How you'll learn";
            var currentHowYoullBeAssessed = "How you'll be assessed";
            var currentWhatYouCanDoNext = "What you can do next";

            var journeyState = new AddTLevelJourneyModel();
            journeyState.SetTLevel(
                currentlySelectedTLevel.TLevelDefinitionId,
                currentlySelectedTLevel.Name);
            journeyState.SetDescription(
                currentWhoFor,
                currentEntryRequirements,
                currentWhatYoullLearn,
                currentHowYoullLearn,
                currentHowYoullBeAssessed,
                currentWhatYouCanDoNext,
                isComplete: true);
            journeyState.CompletedStages.Should().Be(AddTLevelJourneyCompletedStages.SelectTLevel | AddTLevelJourneyCompletedStages.Description);

            var newSelectedTLevelDefinitionId = tLevelDefinitions.Last().TLevelDefinitionId;
            newSelectedTLevelDefinitionId.Should().NotBe(currentlySelectedTLevel.TLevelDefinitionId);

            var newTLevelExemplarContent = await WithSqlQueryDispatcher(dispatcher =>
                dispatcher.ExecuteQuery(new GetTLevelDefinitionExemplarContent()
                {
                    TLevelDefinitionId = newSelectedTLevelDefinitionId
                }));

            var journeyInstance = CreateJourneyInstance(providerId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("SelectedTLevelDefinitionId", newSelectedTLevelDefinitionId)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);

            journeyState = GetJourneyInstance<AddTLevelJourneyModel>(journeyInstanceId).State;

            using (new AssertionScope())
            {
                journeyState.WhoFor.Should().Be(newTLevelExemplarContent.WhoFor);
                journeyState.EntryRequirements.Should().Be(newTLevelExemplarContent.EntryRequirements);
                journeyState.WhatYoullLearn.Should().Be(newTLevelExemplarContent.WhatYoullLearn);
                journeyState.HowYoullLearn.Should().Be(newTLevelExemplarContent.HowYoullLearn);
                journeyState.HowYoullBeAssessed.Should().Be(newTLevelExemplarContent.HowYoullBeAssessed);
                journeyState.WhatYouCanDoNext.Should().Be(newTLevelExemplarContent.WhatYouCanDoNext);
                journeyState.CompletedStages.Should().Be(AddTLevelJourneyCompletedStages.SelectTLevel);
            }
        }

        [Fact]
        public async Task Post_SelectedTLevelIsSameAsCurrent_DoesNotOverwriteDescriptionOrResetJourneyStage()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var selectedTLevel = tLevelDefinitions.First();

            var whoFor = "Who for";
            var entryRequirements = "Entry requirements";
            var whatYoullLearn = "What you'll learn";
            var howYoullLearn = "How you'll learn";
            var howYoullBeAssessed = "How you'll be assessed";
            var whatYouCanDoNext = "What you can do next";

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
            journeyState.CompletedStages.Should().Be(AddTLevelJourneyCompletedStages.SelectTLevel | AddTLevelJourneyCompletedStages.Description);

            var journeyInstance = CreateJourneyInstance(providerId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("SelectedTLevelDefinitionId", selectedTLevel.TLevelDefinitionId)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);

            journeyState = GetJourneyInstance<AddTLevelJourneyModel>(journeyInstanceId).State;

            using (new AssertionScope())
            {
                journeyState.WhoFor.Should().Be(whoFor);
                journeyState.EntryRequirements.Should().Be(entryRequirements);
                journeyState.WhatYoullLearn.Should().Be(whatYoullLearn);
                journeyState.HowYoullLearn.Should().Be(howYoullLearn);
                journeyState.HowYoullBeAssessed.Should().Be(howYoullBeAssessed);
                journeyState.WhatYouCanDoNext.Should().Be(whatYouCanDoNext);
                journeyState.CompletedStages.Should().Be(AddTLevelJourneyCompletedStages.SelectTLevel | AddTLevelJourneyCompletedStages.Description);
            }
        }
    }
}
