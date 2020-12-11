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
        [InlineData(ProviderType.Apprenticeships)]
        [InlineData(ProviderType.FE)]
        [InlineData(ProviderType.FE | ProviderType.Apprenticeships)]
        public async Task List_Get_ProviderIsNotTLevelsProvider_ReturnsForbidden(ProviderType providerType)
        {
            //Arange
            var providerId = await TestData.CreateProvider(
                providerType: providerType);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/list?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task List_Get_WithNoTLevels_ReturnsExpectedContent()
        {
            //Arange
            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/list?providerId={providerId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.Body.TextContent.Should().Contain("You have no T Levels.");
        }

        [Fact]
        public async Task List_Get_WithTLevels_ReturnsExpectedContent()
        {
            //Arange
            var tLevelDefinitions = await TestData.CreateInitialTLevelDefinitions();

            var providerTLevelDefinitionIds = tLevelDefinitions.Select(tld => tld.TLevelDefinitionId).ToArray();

            var providerId = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                tLevelDefinitionIds: providerTLevelDefinitionIds);

            var venueId1 = await TestData.CreateVenue(providerId, venueName: "TestVenue1");
            var venueId2 = await TestData.CreateVenue(providerId, venueName: "TestVenue2");

            var user = await TestData.CreateUser();

            var tLevel1 = await CreateTLevel(providerId, tLevelDefinitions.First().TLevelDefinitionId, new DateTime(2021, 09, 01), new[] { venueId1 }, user, 1);
            var tLevel2 = await CreateTLevel(providerId, tLevelDefinitions.Skip(1).First().TLevelDefinitionId, new DateTime(2021, 03, 10), new[] { venueId1, venueId2 }, user, 2);
            var tLevel3 = await CreateTLevel(providerId, tLevelDefinitions.First().TLevelDefinitionId, new DateTime(2022, 09, 03), new[] { venueId2 }, user, 3);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/t-levels/list?providerId={providerId}");

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

        private Task<TLevel> CreateTLevel(Guid providerId, Guid tLevelDefinitionId, DateTime startDate, IEnumerable<Guid> locationVenueIds, UserInfo userInfo, int seed) =>
            TestData.CreateTLevel(
                providerId,
                tLevelDefinitionId,
                $"TestWhoFor{seed}",
                $"TestEntryRequirements{seed}",
                $"TestWhatYoullLearn{seed}",
                $"TestHowYoullLearn{seed}",
                $"TestHowYoullBeAssessed{seed}",
                $"TestWhatYouCanDoNext{seed}",
                $"TestYourReference{seed}",
                startDate,
                locationVenueIds?.ToArray(),
                $"http://testwebsite{seed}.com",
                userInfo);    }
}
