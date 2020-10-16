using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
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
        public async Task Get_ProviderIsFEOnlyReturnsBadRequest()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.FE);

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeships/find-standard?providerId={providerId}&returnUrl=%2Fcallback");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetSearch_NotEnoughCharactersReturnsError()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Both);

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeships/find-standard/search?providerId={providerId}&returnUrl=%2Fcallback&Search=h");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError(
                "Search",
                "Name or keyword for the apprenticeship this training is for must be 3 characters or more");
        }

        [Fact]
        public async Task GetSearch_RendersSearchResults()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Both);

            await TestData.CreateStandard(standardCode: 123, version: 1, standardName: "Hairdressing");
            await TestData.CreateStandard(standardCode: 456, version: 2, standardName: "Hair");

            // Framework should no longer be returned in search results
            await TestData.CreateFramework(frameworkCode: 789, progType: 2, pathwayCode: 3, nasTitle: "Haircuts");

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeships/find-standard/search?providerId={providerId}&returnUrl=%2Fcallback&Search=hair");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();

            Assert.Equal(
                "Found 2 results for hair",
                doc.GetElementById("pttcd-apprenticeships__find-provision__results-count").TextContent.Trim());
        }

        [Fact]
        public async Task GetSelect_ValidRequest_UpdatesParentStateAndRedirects()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Both);

            await TestData.CreateStandard(standardCode: 456, version: 2, standardName: "Hair");

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeships/find-standard/search?providerId={providerId}&returnUrl=%2Fcallback&Search=hair");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();

            Assert.Equal(
                "/callback?standardCode=456&version=2",
                doc.GetAllElementsByTestId("choose-apprenticeship-link")[0].GetAttribute("href"));
        }
    }
}
