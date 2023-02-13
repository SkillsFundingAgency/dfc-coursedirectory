using System;
using System.Collections.Generic;
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
using FormFlow;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.TLevels.AddTLevel
{
    public class DescriptionTests : AddTLevelTestBase
    {
        public DescriptionTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(ProviderType.None)]
        [InlineData(ProviderType.Apprenticeships)]
        [InlineData(ProviderType.FE)]
        public async Task Get_ProviderIsNotTLevelsProvider_ReturnsForbidden(ProviderType providerType)
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var provider = await TestData.CreateProvider(providerType: providerType);

            var venueId = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo());

            var selectedTLevel = tLevelDefinitions.First();

            var exemplarContent = await WithSqlQueryDispatcher(dispatcher =>
                dispatcher.ExecuteQuery(new GetTLevelDefinitionExemplarContent()
                {
                    TLevelDefinitionId = selectedTLevel.TLevelDefinitionId
                }));

            var journeyState = new AddTLevelJourneyModel();

            journeyState.SetTLevel(
                selectedTLevel.TLevelDefinitionId,
                selectedTLevel.Name,
                exemplarContent);

            var journeyInstance = CreateJourneyInstance(provider.ProviderId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/add/description?providerId={provider.ProviderId}&ffiid={journeyInstanceId.UniqueKey}");

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
                $"/t-levels/add/description?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}");
        });

        [Fact]
        public async Task Get_JourneyStateIsNotValid_ReturnsBadRequest()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venueId = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo());

            var selectedTLevel = tLevelDefinitions.First();

            var journeyState = new AddTLevelJourneyModel();

            journeyState.CompletedStages.Should().Be(AddTLevelJourneyCompletedStages.None);

            var journeyInstance = CreateJourneyInstance(provider.ProviderId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/add/description?providerId={provider.ProviderId}&ffiid={journeyInstanceId.UniqueKey}");

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

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venueId = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo());

            var selectedTLevel = tLevelDefinitions.First();

            var exemplarContent = await WithSqlQueryDispatcher(dispatcher =>
                dispatcher.ExecuteQuery(new GetTLevelDefinitionExemplarContent()
                {
                    TLevelDefinitionId = selectedTLevel.TLevelDefinitionId
                }));

            var journeyState = new AddTLevelJourneyModel();

            journeyState.SetTLevel(
                selectedTLevel.TLevelDefinitionId,
                selectedTLevel.Name,
                exemplarContent);

            var journeyInstance = CreateJourneyInstance(provider.ProviderId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/add/description?providerId={provider.ProviderId}&ffiid={journeyInstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();

            using (new AssertionScope())
            {
                doc.GetElementById("WhoFor").InnerHtml.Trim().Should().Be(exemplarContent.WhoFor);
                doc.GetElementById("EntryRequirements").InnerHtml.Trim().Should().Be(exemplarContent.EntryRequirements);
                doc.GetElementById("WhatYoullLearn").InnerHtml.Trim().Should().Be(exemplarContent.WhatYoullLearn);
                doc.GetElementById("HowYoullLearn").InnerHtml.Trim().Should().Be(exemplarContent.HowYoullLearn);
                doc.GetElementById("HowYoullBeAssessed").InnerHtml.Trim().Should().Be(exemplarContent.HowYoullBeAssessed);
                doc.GetElementById("WhatYouCanDoNext").InnerHtml.Trim().Should().Be(exemplarContent.WhatYouCanDoNext);
            }
        }

        [Theory]
        [InlineData(ProviderType.None)]
        [InlineData(ProviderType.Apprenticeships)]
        [InlineData(ProviderType.FE)]
        public async Task Post_ProviderIsNotTLevelsProvider_ReturnsForbidden(ProviderType providerType)
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var provider = await TestData.CreateProvider(providerType: providerType);

            var venueId = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo());

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

            var journeyInstance = CreateJourneyInstance(provider.ProviderId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add/description?providerId={provider.ProviderId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhoFor", whoFor)
                    .Add("EntryRequirements", entryRequirements)
                    .Add("WhatYoullLearn", whatYoullLearn)
                    .Add("HowYoullLearn", howYoullLearn)
                    .Add("HowYoullBeAssessed", howYoullBeAssessed)
                    .Add("WhatYouCanDoNext", whatYouCanDoNext)
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
            var (providerId, _, journeyInstanceId) = info;

            var whoFor = "Who for";
            var entryRequirements = "Entry requirements";
            var whatYoullLearn = "What you'll learn";
            var howYoullLearn = "How you'll learn";
            var howYoullBeAssessed = "How you'll be assessed";
            var whatYouCanDoNext = "What you can do next";

            return new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add/description?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhoFor", whoFor)
                    .Add("EntryRequirements", entryRequirements)
                    .Add("WhatYoullLearn", whatYoullLearn)
                    .Add("HowYoullLearn", howYoullLearn)
                    .Add("HowYoullBeAssessed", howYoullBeAssessed)
                    .Add("WhatYouCanDoNext", whatYouCanDoNext)
                    .ToContent()
            };
        });

        [Fact]
        public async Task Post_JourneyStateIsNotValid_ReturnsBadRequest()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venueId = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo());

            var selectedTLevel = tLevelDefinitions.First();
            var whoFor = "Who for";
            var entryRequirements = "Entry requirements";
            var whatYoullLearn = "What you'll learn";
            var howYoullLearn = "How you'll learn";
            var howYoullBeAssessed = "How you'll be assessed";
            var whatYouCanDoNext = "What you can do next";

            var journeyState = new AddTLevelJourneyModel();

            journeyState.CompletedStages.Should().Be(AddTLevelJourneyCompletedStages.None);

            var journeyInstance = CreateJourneyInstance(provider.ProviderId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add/description?providerId={provider.ProviderId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhoFor", whoFor)
                    .Add("EntryRequirements", entryRequirements)
                    .Add("WhatYoullLearn", whatYoullLearn)
                    .Add("HowYoullLearn", howYoullLearn)
                    .Add("HowYoullBeAssessed", howYoullBeAssessed)
                    .Add("WhatYouCanDoNext", whatYouCanDoNext)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [ClassData(typeof(ValidationErrorsData))]
        public async Task Post_InvalidData_RendersError(
            string whoFor,
            string entryRequirements,
            string whatYoullLearn,
            string howYoullLearn,
            string howYoullBeAssessed,
            string whatYouCanDoNext,
            string expectedErrorField,
            string expectedErrorMessage)
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venueId = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo());

            var selectedTLevel = tLevelDefinitions.First();

            var journeyState = new AddTLevelJourneyModel();

            journeyState.SetTLevel(
                selectedTLevel.TLevelDefinitionId,
                selectedTLevel.Name);

            var journeyInstance = CreateJourneyInstance(provider.ProviderId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add/description?providerId={provider.ProviderId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhoFor", whoFor)
                    .Add("EntryRequirements", entryRequirements)
                    .Add("WhatYoullLearn", whatYoullLearn)
                    .Add("HowYoullLearn", howYoullLearn)
                    .Add("HowYoullBeAssessed", howYoullBeAssessed)
                    .Add("WhatYouCanDoNext", whatYouCanDoNext)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError(expectedErrorField, expectedErrorMessage);

            GetJourneyInstance<AddTLevelJourneyModel>(journeyInstanceId).State
                .CompletedStages.Should().NotHaveFlag(AddTLevelJourneyCompletedStages.Description);
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

            var venueId = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo());

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

            var journeyInstance = CreateJourneyInstance(provider.ProviderId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add/description?providerId={provider.ProviderId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhoFor", whoFor)
                    .Add("EntryRequirements", entryRequirements)
                    .Add("WhatYoullLearn", whatYoullLearn)
                    .Add("HowYoullLearn", howYoullLearn)
                    .Add("HowYoullBeAssessed", howYoullBeAssessed)
                    .Add("WhatYouCanDoNext", whatYouCanDoNext)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString
                .Should().Be($"/t-levels/add/details?providerId={provider.ProviderId}&ffiid={journeyInstanceId.UniqueKey}");

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

        public class ValidationErrorsData :
            TheoryData<string, string, string, string, string, string, string, string>
        {
            public ValidationErrorsData()
            {
                Add(
                    whoFor: "",
                    expectedErrorField: "WhoFor",
                    expectedErrorMessage: "Enter who this T Level is for");

                Add(
                    whoFor: new string('x', 501),
                    expectedErrorField: "WhoFor",
                    expectedErrorMessage: "Who this T Level is for must be 500 characters or fewer");

                Add(
                    entryRequirements: new string('x', 501),
                    expectedErrorField: "EntryRequirements",
                    expectedErrorMessage: "Entry requirements must be 500 characters or fewer");

                Add(
                    whatYoullLearn: new string('x', 1501),
                    expectedErrorField: "WhatYoullLearn",
                    expectedErrorMessage: "What you'll learn must be 1500 characters or fewer");
                
                Add(
                    howYoullLearn: new string('x', 501),
                    expectedErrorField: "HowYoullLearn",
                    expectedErrorMessage: "How you'll learn must be 500 characters or fewer");
                
                Add(
                    howYoullBeAssessed: new string('x', 501),
                    expectedErrorField: "HowYoullBeAssessed",
                    expectedErrorMessage: "How you'll be assessed must be 500 characters or fewer");
                
                Add(
                    whatYouCanDoNext: new string('x', 501),
                    expectedErrorField: "WhatYouCanDoNext",
                    expectedErrorMessage: "What you can do next must be 500 characters or fewer");
            }

            public new void Add(
                string expectedErrorField,
                string expectedErrorMessage,
                string whoFor = "Who for",
                string entryRequirements = "Entry requirements",
                string whatYoullLearn = "What you'll learn",
                string howYoullLearn = "How you'll learn",
                string howYoullBeAssessed = "How you'll be assessed",
                string whatYouCanDoNext = "What you can do next")
            {
                base.Add(
                    whoFor,
                    entryRequirements,
                    whatYoullLearn,
                    howYoullLearn,
                    howYoullBeAssessed,
                    whatYouCanDoNext,
                    expectedErrorField,
                    expectedErrorMessage);
            }
        }
    }
}
