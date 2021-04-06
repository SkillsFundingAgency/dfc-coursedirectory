using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.TLevels.DeleteTLevel;
using FluentAssertions;
using FluentAssertions.Execution;
using FormFlow;
using OneOf;
using OneOf.Types;
using Xunit;
using SqlQueries = Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.TLevels.DeleteTLevel
{
    public class DeleteTLevelTests : MvcTestBase
    {
        public DeleteTLevelTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_TLevelDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var tLevelId = Guid.NewGuid();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/t-levels/{tLevelId}/delete");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_UserCannotAccessTLevel_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var anotherProvider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

            var tLevel = await TestData.CreateTLevel(
                provider.ProviderId,
                tLevelDefinitions.First().TLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                createdBy: User.ToUserInfo());

            var request = new HttpRequestMessage(HttpMethod.Get, $"/t-levels/{tLevel.TLevelId}/delete");

            await User.AsTestUser(userType, anotherProvider.ProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(provider.ProviderId, venueName: "T Level venue")).Id;

            var tLevelDefinition = tLevelDefinitions.First();

            var yourReference = "YOUR-REF";
            var startDate = new DateTime(2021, 10, 1);

            var tLevel = await TestData.CreateTLevel(
                provider.ProviderId,
                tLevelDefinition.TLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                yourReference: yourReference,
                startDate: startDate,
                createdBy: User.ToUserInfo());

            var request = new HttpRequestMessage(HttpMethod.Get, $"/t-levels/{tLevel.TLevelId}/delete");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();

            using (new AssertionScope())
            {
                doc.GetSummaryListValueWithKey("T Level name").Should().Be(tLevelDefinition.Name);
                doc.GetSummaryListValueWithKey("Your reference").Should().Be(yourReference);
                doc.GetSummaryListValueWithKey("Start date").Should().Be(startDate.ToString("d MMMM yyyy"));
                doc.GetAllElementsByTestId("tlevel-location-names").Should().Contain(e => e.InnerHtml == "T Level venue");
            }
        }

        [Fact]
        public async Task Post_CourseDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var tLevelId = Guid.NewGuid();

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"/t-levels/{tLevelId}/delete")
            {
                Content = requestContent
            };

            CreateJourneyInstance(tLevelId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Post_UserCannotAccessCourse_ReturnsForbidden(TestUserType userType)
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var anotherProvider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(provider.ProviderId, venueName: "T Level venue")).Id;

            var tLevelDefinition = tLevelDefinitions.First();

            var yourReference = "YOUR-REF";
            var startDate = new DateTime(2021, 10, 1);

            var tLevel = await TestData.CreateTLevel(
                provider.ProviderId,
                tLevelDefinition.TLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                yourReference: yourReference,
                startDate: startDate,
                createdBy: User.ToUserInfo());

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"/t-levels/{tLevel.TLevelId}/delete")
            {
                Content = requestContent
            };

            CreateJourneyInstance(tLevel.TLevelId);

            await User.AsTestUser(userType, anotherProvider.ProviderId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_NotConfirmed_ReturnsError()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(provider.ProviderId, venueName: "T Level venue")).Id;

            var tLevelDefinition = tLevelDefinitions.First();

            var yourReference = "YOUR-REF";
            var startDate = new DateTime(2021, 10, 1);

            var tLevel = await TestData.CreateTLevel(
                provider.ProviderId,
                tLevelDefinition.TLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                yourReference: yourReference,
                startDate: startDate,
                createdBy: User.ToUserInfo());

            var requestContent = new FormUrlEncodedContentBuilder()
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"/t-levels/{tLevel.TLevelId}/delete")
            {
                Content = requestContent
            };

            CreateJourneyInstance(tLevel.TLevelId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            var doc = await response.GetDocument();
            doc.AssertHasError("Confirm", "Confirm you want to delete the T Level");
        }

        [Fact]
        public async Task Post_ValidRequest_DeletesCourseRunAndRedirects()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(provider.ProviderId, venueName: "T Level venue")).Id;

            var tLevelDefinition = tLevelDefinitions.First();

            var yourReference = "YOUR-REF";
            var startDate = new DateTime(2021, 10, 1);

            var tLevel = await TestData.CreateTLevel(
                provider.ProviderId,
                tLevelDefinition.TLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                yourReference: yourReference,
                startDate: startDate,
                createdBy: User.ToUserInfo());

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Confirm", "true")
                .ToContent();

            var request = new HttpRequestMessage(HttpMethod.Post, $"/t-levels/{tLevel.TLevelId}/delete")
            {
                Content = requestContent
            };

            CreateJourneyInstance(tLevel.TLevelId);

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);

            response.Headers.Location.OriginalString.Should()
                .Be($"/t-levels/{tLevel.TLevelId}/delete/success?providerId={provider.ProviderId}");

            SqlQuerySpy.VerifyQuery<SqlQueries.DeleteTLevel, OneOf<NotFound, Success>>(q => q.TLevelId == tLevel.TLevelId);
        }

        [Fact]
        public async Task GetDeleted_RendersExpectedTLevelName()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(provider.ProviderId, venueName: "T Level venue")).Id;

            var tLevelDefinition = tLevelDefinitions.First();

            var yourReference = "YOUR-REF";
            var startDate = new DateTime(2021, 10, 1);

            var tLevel = await TestData.CreateTLevel(
                provider.ProviderId,
                tLevelDefinition.TLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                yourReference: yourReference,
                startDate: startDate,
                createdBy: User.ToUserInfo());

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new SqlQueries.DeleteTLevel()
                {
                    TLevelId = tLevel.TLevelId,
                    DeletedOn = Clock.UtcNow,
                    DeletedBy = User.ToUserInfo()
                }));

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/{tLevel.TLevelId}/delete/success?providerId={provider.ProviderId}");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var journeyInstance = CreateJourneyInstance(
                tLevel.TLevelId,
                new JourneyModel()
                {
                    TLevelName = tLevelDefinition.Name,
                    ProviderId = provider.ProviderId,
                    YourReference = yourReference
                });

            journeyInstance.Complete();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            doc.GetElementByTestId("TLevelName").TextContent.Should().Be(tLevelDefinition.Name);
        }

        [Fact]
        public async Task GetDeleted_NoOtherLiveTLevels_DoesNotRenderViewEditLink()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(provider.ProviderId, venueName: "T Level venue")).Id;

            var tLevelDefinition = tLevelDefinitions.First();

            var yourReference = "YOUR-REF";
            var startDate = new DateTime(2021, 10, 1);

            var tLevel = await TestData.CreateTLevel(
                provider.ProviderId,
                tLevelDefinition.TLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                yourReference: yourReference,
                startDate: startDate,
                createdBy: User.ToUserInfo());

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new SqlQueries.DeleteTLevel()
                {
                    TLevelId = tLevel.TLevelId,
                    DeletedOn = Clock.UtcNow,
                    DeletedBy = User.ToUserInfo()
                }));

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/{tLevel.TLevelId}/delete/success?providerId={provider.ProviderId}");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var journeyInstance = CreateJourneyInstance(
                tLevel.TLevelId,
                new JourneyModel()
                {
                    TLevelName = tLevelDefinition.Name,
                    ProviderId = provider.ProviderId,
                    YourReference = yourReference
                });

            journeyInstance.Complete();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            doc.GetElementByTestId("ViewEditLink").Should().BeNull();
        }

        [Fact]
        public async Task GetDeleted_HasOtherLiveCourseRuns_DoesRenderViewEditCopyLink()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(provider.ProviderId, venueName: "T Level venue")).Id;

            var tLevelDefinition = tLevelDefinitions.First();

            var yourReference = "YOUR-REF";
            var startDate = new DateTime(2021, 10, 1);

            var tLevel = await TestData.CreateTLevel(
                provider.ProviderId,
                tLevelDefinition.TLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                yourReference: yourReference,
                startDate: startDate,
                createdBy: User.ToUserInfo());

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new SqlQueries.DeleteTLevel()
                {
                    TLevelId = tLevel.TLevelId,
                    DeletedOn = Clock.UtcNow,
                    DeletedBy = User.ToUserInfo()
                }));

            var anotherTLevel = await TestData.CreateTLevel(
                provider.ProviderId,
                tLevelDefinitions.Last().TLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                createdBy: User.ToUserInfo());

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/{tLevel.TLevelId}/delete/success?providerId={provider.ProviderId}");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var journeyInstance = CreateJourneyInstance(
                tLevel.TLevelId,
                new JourneyModel()
                {
                    TLevelName = tLevelDefinition.Name,
                    ProviderId = provider.ProviderId,
                    YourReference = yourReference
                });

            journeyInstance.Complete();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            doc.GetElementByTestId("ViewEditLink").Should().NotBeNull();
        }

        [Fact]
        public async Task GetDeleted_WithNonEmptyYourReference_RendersYourReference()
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(provider.ProviderId, venueName: "T Level venue")).Id;

            var tLevelDefinition = tLevelDefinitions.First();

            var yourReference = "YOUR-REF";
            var startDate = new DateTime(2021, 10, 1);

            var tLevel = await TestData.CreateTLevel(
                provider.ProviderId,
                tLevelDefinition.TLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                yourReference: yourReference,
                startDate: startDate,
                createdBy: User.ToUserInfo());

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new SqlQueries.DeleteTLevel()
                {
                    TLevelId = tLevel.TLevelId,
                    DeletedOn = Clock.UtcNow,
                    DeletedBy = User.ToUserInfo()
                }));

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/{tLevel.TLevelId}/delete/success?providerId={provider.ProviderId}");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var journeyInstance = CreateJourneyInstance(
                tLevel.TLevelId,
                new JourneyModel()
                {
                    TLevelName = tLevelDefinition.Name,
                    ProviderId = provider.ProviderId,
                    YourReference = yourReference
                });

            journeyInstance.Complete();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            doc.GetElementByTestId("YourReference").TextContent.Should().Be(yourReference);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public async Task GetDeleted_WithEmptyYourReference_DoesNotRenderYourReference(string yourReference)
        {
            // Arrange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray());

            var venueId = (await TestData.CreateVenue(provider.ProviderId, venueName: "T Level venue")).Id;

            var tLevelDefinition = tLevelDefinitions.First();

            var startDate = new DateTime(2021, 10, 1);

            var tLevel = await TestData.CreateTLevel(
                provider.ProviderId,
                tLevelDefinition.TLevelDefinitionId,
                locationVenueIds: new[] { venueId },
                yourReference: yourReference,
                startDate: startDate,
                createdBy: User.ToUserInfo());

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
                new SqlQueries.DeleteTLevel()
                {
                    TLevelId = tLevel.TLevelId,
                    DeletedOn = Clock.UtcNow,
                    DeletedBy = User.ToUserInfo()
                }));

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/{tLevel.TLevelId}/delete/success?providerId={provider.ProviderId}");

            await User.AsProviderUser(provider.ProviderId, ProviderType.FE);

            var journeyInstance = CreateJourneyInstance(
                tLevel.TLevelId,
                new JourneyModel()
                {
                    TLevelName = tLevelDefinition.Name,
                    ProviderId = provider.ProviderId,
                    YourReference = yourReference
                });

            journeyInstance.Complete();

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            doc.GetElementByTestId("YourReference").Should().BeNull();
        }

        private JourneyInstance<JourneyModel> CreateJourneyInstance(
            Guid tLevelId,
            JourneyModel journeyModel = null)
        {
            return CreateJourneyInstance(
                "DeleteTLevel",
                configureKeys: keysBuilder => keysBuilder
                    .With("tLevelId", tLevelId),
                journeyModel ?? new JourneyModel());
        }
    }
}
