using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Apprenticeships
{
    public class FindStandardTests : MvcTestBase
    {
        public FindStandardTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_ProviderIsFEOnlyReturnsForbidden()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.FE);

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeships/find-standard?providerId={provider.ProviderId}&returnUrl=%2Fcallback");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetSearch_NotEnoughCharactersReturnsError()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.FE | ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeships/find-standard/search?providerId={provider.ProviderId}&returnUrl=%2Fcallback&Search=h");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError(
                "Search",
                "Name or keyword for the apprenticeship this training is for must be 3 characters or more");
        }

        [Fact]
        public async Task GetSearch_RendersSearchResults()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.FE | ProviderType.Apprenticeships);

            await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "Hairdressing");
            await TestData.CreateStandard(standardCode: 456, version: 2, standardName: "Hair");

            // Framework should no longer be returned in search results
            await TestData.CreateFramework(frameworkCode: 789, progType: 2, pathwayCode: 3, nasTitle: "Haircuts");

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeships/find-standard/search?providerId={provider.ProviderId}&returnUrl=%2Fcallback&Search=hair");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();

            doc.GetElementById("pttcd-apprenticeships__find-provision__results-count").TextContent.Trim().Should()
                .Be("Found 2 results for hair");
        }

        [Fact]
        public async Task GetSelect_ValidRequest_UpdatesParentStateAndRedirects()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.FE | ProviderType.Apprenticeships);

            await TestData.CreateStandard(standardCode: 456, version: 2, standardName: "Hair");

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeships/find-standard/search?providerId={provider.ProviderId}&returnUrl=%2Fcallback&Search=hair");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            doc.GetAllElementsByTestId("choose-apprenticeship-link")[0].GetAttribute("href").Should()
                .Be("/callback?standardCode=456&version=2");
        }
    }
}
