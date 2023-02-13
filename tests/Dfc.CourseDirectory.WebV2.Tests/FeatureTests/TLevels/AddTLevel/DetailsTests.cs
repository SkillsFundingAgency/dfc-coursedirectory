using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.TLevels.AddTLevel;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.TLevels.AddTLevel
{
    public class DetailsTests : AddTLevelTestBase
    {
        public DetailsTests(CourseDirectoryApplicationFactory factory)
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

            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

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

            var journeyInstance = CreateJourneyInstance(provider.ProviderId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/add/details?providerId={provider.ProviderId}&ffiid={journeyInstanceId.UniqueKey}");

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
                $"/t-levels/add/details?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}");
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

            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

            var selectedTLevel = tLevelDefinitions.First();

            var journeyState = new AddTLevelJourneyModel();

            journeyState.CompletedStages.Should().Be(AddTLevelJourneyCompletedStages.None);

            var journeyInstance = CreateJourneyInstance(provider.ProviderId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/add/details?providerId={provider.ProviderId}&ffiid={journeyInstanceId.UniqueKey}");

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

            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;
            var anotherVenueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: "Second Venue")).VenueId;

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

            var journeyInstance = CreateJourneyInstance(provider.ProviderId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/add/details?providerId={provider.ProviderId}&ffiid={journeyInstanceId.UniqueKey}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();

            using (new AssertionScope())
            {
                doc.GetElementById("YourReference").GetAttribute("value").Should().Be(yourReference);
                doc.GetElementById("StartDate.Day").GetAttribute("value").Should().Be(startDate.Day.ToString());
                doc.GetElementById("StartDate.Month").GetAttribute("value").Should().Be(startDate.Month.ToString());
                doc.GetElementById("StartDate.Year").GetAttribute("value").Should().Be(startDate.Year.ToString());
                doc.GetElementByTestId($"LocationVenueIds-{venueId}").GetAttribute("checked").Should().Be("checked");
                doc.GetElementByTestId($"LocationVenueIds-{anotherVenueId}").GetAttribute("checked").Should().NotBe("checked");
                doc.GetElementById("Website").GetAttribute("value").Should().Be(website);
            }
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

            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;
            var anotherVenueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: "Second Venue")).VenueId;

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

            var journeyInstance = CreateJourneyInstance(provider.ProviderId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/add/details?providerId={provider.ProviderId}&ffiid={journeyInstanceId.UniqueKey}&venueId={venueId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.GetElementByTestId($"LocationVenueIds-{venueId}").GetAttribute("checked").Should().Be("checked");
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

            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

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

            var journeyInstance = CreateJourneyInstance(provider.ProviderId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add/details?providerId={provider.ProviderId}&ffiid={journeyInstanceId.UniqueKey}")
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
                $"/t-levels/add/details?providerId={providerId}&ffiid={journeyInstanceId.UniqueKey}")
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

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

            var yourReference = "YOUR-REF";
            var startDate = new DateTime(2021, 4, 1);
            var website = "http://example.com/tlevel";

            var journeyState = new AddTLevelJourneyModel();

            journeyState.CompletedStages.Should().Be(AddTLevelJourneyCompletedStages.None);

            var journeyInstance = CreateJourneyInstance(provider.ProviderId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add/details?providerId={provider.ProviderId}&ffiid={journeyInstanceId.UniqueKey}")
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

        [Theory]
        [ClassData(typeof(ValidationErrorsData))]
        public async Task Post_InvalidData_RendersError(
            string yourReference,
            DateTime? startDate,
            bool addTLevelWithSameStartDate,
            bool populateLocationVenueIds,
            string website,
            string expectedErrorField,
            string expectedErrorMessage)
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var authorizedTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: authorizedTLevelDefinitionIds);

            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

            var selectedTLevel = tLevelDefinitions.First();

            if (addTLevelWithSameStartDate)
            {
                await TestData.CreateTLevel(
                    provider.ProviderId,
                    selectedTLevel.TLevelDefinitionId,
                    startDate: startDate.Value,
                    locationVenueIds: new[] { venueId },
                    createdBy: User.ToUserInfo());
            }

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

            var journeyInstance = CreateJourneyInstance(provider.ProviderId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add/details?providerId={provider.ProviderId}&ffiid={journeyInstanceId.UniqueKey}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("YourReference", yourReference)
                    .Add("StartDate.Day", startDate?.Day)
                    .Add("StartDate.Month", startDate?.Month)
                    .Add("StartDate.Year", startDate?.Year)
                    .Add("LocationVenueIds", populateLocationVenueIds ? venueId.ToString() : null)
                    .Add("Website", website)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError(expectedErrorField, expectedErrorMessage);

            GetJourneyInstance<AddTLevelJourneyModel>(journeyInstanceId).State
                .CompletedStages.Should().NotHaveFlag(AddTLevelJourneyCompletedStages.Details);
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

            var venueId = (await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo())).VenueId;

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

            var journeyInstance = CreateJourneyInstance(provider.ProviderId, journeyState);
            var journeyInstanceId = journeyInstance.InstanceId;

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/t-levels/add/details?providerId={provider.ProviderId}&ffiid={journeyInstanceId.UniqueKey}")
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
            response.Headers.Location.OriginalString
                .Should().Be($"/t-levels/add/check-publish?providerId={provider.ProviderId}&ffiid={journeyInstanceId.UniqueKey}");

            journeyState = GetJourneyInstance<AddTLevelJourneyModel>(journeyInstanceId).State;

            using (new AssertionScope())
            {
                journeyState.YourReference.Should().Be(yourReference);
                journeyState.StartDate.Should().Be(startDate);
                journeyState.LocationVenueIds.Should().BeEquivalentTo(new[] { venueId });
                journeyState.Website.Should().Be(website);

                journeyState.CompletedStages.Should().Be(
                    AddTLevelJourneyCompletedStages.SelectTLevel | 
                    AddTLevelJourneyCompletedStages.Description |
                    AddTLevelJourneyCompletedStages.Details);
            }
        }

        public class ValidationErrorsData :
            TheoryData<string, DateTime?, bool, bool, string, string, string>
        {
            public ValidationErrorsData()
            {
                Add(
                    yourReference: new string('x', 256),
                    startDate: new DateTime(2021, 10, 1),
                    expectedErrorField: "YourReference",
                    expectedErrorMessage: "Your reference must be 255 characters or fewer");

                Add(
                    startDate: null,
                    expectedErrorField: "StartDate",
                    expectedErrorMessage: "Enter a start date");

                Add(
                    startDate: new DateTime(2021, 10, 1),
                    addTLevelWithSameStartDate: true,
                    expectedErrorField: "StartDate",
                    expectedErrorMessage: "Start date already exists");

                Add(
                    populateLocationVenuesIds: false,
                    startDate: new DateTime(2021, 10, 1),
                    expectedErrorField: "LocationVenueIds",
                    expectedErrorMessage: "Select a T Level venue");

                Add(
                    website: "tlevel",
                    startDate: new DateTime(2021, 10, 1),
                    expectedErrorField: "Website",
                    expectedErrorMessage: "Website must be a real webpage");

                Add(
                    website: "www.example.com/tlevel" + new string('x', 234),
                    startDate: new DateTime(2021, 10, 1),
                    expectedErrorField: "Website",
                    expectedErrorMessage: "T Level webpage must be 255 characters or fewer");

                Add(
                    website: "",
                    startDate: new DateTime(2021, 10, 1),
                    expectedErrorField: "Website",
                    expectedErrorMessage: "Enter a webpage");
            }

            public void Add(
                string expectedErrorField,
                string expectedErrorMessage,
                string yourReference = "YOUR-REF",
                DateTime? startDate = null,
                bool addTLevelWithSameStartDate = false,
                bool populateLocationVenuesIds = true,
                string website = "www.example.com/tlevel")
            {
                Add(
                    yourReference,
                    startDate,
                    addTLevelWithSameStartDate,
                    populateLocationVenuesIds,
                    website,
                    expectedErrorField,
                    expectedErrorMessage);
            }
        }
    }
}
