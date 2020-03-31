using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Models;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Apprenticeships
{
    public class FindStandardOrFrameworkTests : TestBase
    {
        public FindStandardOrFrameworkTests(CourseDirectoryApplicationFactory factory)
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
                $"apprenticeships/find-standard?providerId={providerId}&returnUrl=%2Fnext-page");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetSearch_ProviderIsFEOnlyReturnsBadRequest()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.FE);

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeships/find-standard/search?providerId={providerId}&returnUrl=%2Fnext-page&Search=hair");

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
                $"apprenticeships/find-standard/search?providerId={providerId}&returnUrl=%2Fnext-page&Search=h");

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
            await TestData.CreateFramework(frameworkCode: 789, progType: 2, pathwayCode: 3, nasTitle: "Haircuts");

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeships/find-standard/search?providerId={providerId}&returnUrl=%2Fnext-page&Search=hair");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();

            Assert.Equal(
                "Found 3 results for hair",
                doc.GetElementById("pttcd-apprenticeships__find-provision__results-count").TextContent.Trim());
        }
    }
}
