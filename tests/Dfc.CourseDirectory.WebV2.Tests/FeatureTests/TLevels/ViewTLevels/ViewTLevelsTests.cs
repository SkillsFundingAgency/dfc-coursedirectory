using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.TLevels.ViewTLevels
{
    public class ViewTLevelsTests : MvcTestBase
    {
        public ViewTLevelsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(ProviderType.None)]
        [InlineData(ProviderType.FE)]
        public async Task List_Get_ProviderIsNotTLevelsProvider_ReturnsForbidden(ProviderType providerType)
        {
            //Arange
            var provider = await TestData.CreateProvider(
                providerType: providerType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task List_Get_WithNoTLevels_ReturnsExpectedContent()
        {
            //Arange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.Body.TextContent.Should().Contain("You have no T Levels");
        }

        [Fact]
        public async Task List_Get_WithTLevels_ReturnsExpectedContent()
        {
            //Arange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: providerTLevelDefinitionIds);

            var venue1 = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: "TestVenue1");
            var venue2 = await TestData.CreateVenue(provider.ProviderId, createdBy: User.ToUserInfo(), venueName: "TestVenue2");

            var user = await TestData.CreateUser();

            var tLevel1 = await CreateTLevel(provider.ProviderId, tLevelDefinitions.First().TLevelDefinitionId, new[] { venue1.VenueId }, user, DateTime.UtcNow.AddMonths(1).Date, 1);
            var tLevel2 = await CreateTLevel(provider.ProviderId, tLevelDefinitions.Skip(1).First().TLevelDefinitionId, new[] { venue1.VenueId, venue2.VenueId }, user, DateTime.UtcNow.AddMonths(2).Date, 2);
            var tLevel3 = await CreateTLevel(provider.ProviderId, tLevelDefinitions.First().TLevelDefinitionId, new[] { venue2.VenueId }, user, DateTime.UtcNow.AddMonths(6).Date, 3);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();

            var tLevel1Row = doc.GetElementByTestId($"tLevelRow-{tLevel1.TLevelId}");
            var tLevel2Row = doc.GetElementByTestId($"tLevelRow-{tLevel2.TLevelId}");
            var tLevel3Row = doc.GetElementByTestId($"tLevelRow-{tLevel3.TLevelId}");

            using (new AssertionScope())
            {
                tLevel1Row.Should().NotBeNull();
                tLevel1Row.GetElementByTestId("tLevelTLevel").TextContent.Should().Be(tLevel1.TLevelDefinition.Name);
                tLevel1Row.GetElementByTestId("tLevelStartDate").TextContent.Should().Be(tLevel1.StartDate.ToString("dd MMM yyyy"));
                tLevel1Row.GetElementByTestId("tLevelVenues").TextContent.Should().Be("TestVenue1");

                tLevel2Row.Should().NotBeNull();
                tLevel2Row.GetElementByTestId("tLevelTLevel").TextContent.Should().Be(tLevel2.TLevelDefinition.Name);
                tLevel2Row.GetElementByTestId("tLevelStartDate").TextContent.Should().Be(tLevel2.StartDate.ToString("dd MMM yyyy"));
                tLevel2Row.GetElementByTestId("tLevelVenues").TextContent.Should().Be("Multiple venues");

                tLevel3Row.Should().NotBeNull();
                tLevel3Row.GetElementByTestId("tLevelTLevel").TextContent.Should().Be(tLevel3.TLevelDefinition.Name);
                tLevel3Row.GetElementByTestId("tLevelStartDate").TextContent.Should().Be(tLevel3.StartDate.ToString("dd MMM yyyy"));
                tLevel3Row.GetElementByTestId("tLevelVenues").TextContent.Should().Be("TestVenue2");
            }
        }

        private Task<TLevel> CreateTLevel(Guid providerId, Guid tLevelDefinitionId, IEnumerable<Guid> locationVenueIds, UserInfo userInfo, DateTime startDate, int seed) =>
            TestData.CreateTLevel(
                providerId,
                tLevelDefinitionId,
                locationVenueIds?.ToArray(),
                userInfo,
                startDate,
                $"TestWhoFor{seed}",
                $"TestEntryRequirements{seed}",
                $"TestWhatYoullLearn{seed}",
                $"TestHowYoullLearn{seed}",
                $"TestHowYoullBeAssessed{seed}",
                $"TestWhatYouCanDoNext{seed}",
                $"TestYourReference{seed}",
                $"http://testwebsite{seed}.com");
    }
}
