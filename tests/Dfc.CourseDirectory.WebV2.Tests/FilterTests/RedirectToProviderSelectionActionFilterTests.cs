using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FilterTests
{
    public class RedirectToProviderSelectionActionFilterTests : TestBase
    {
        private readonly HttpClient _httpClientWithAutoRedirects;

        public RedirectToProviderSelectionActionFilterTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
            _httpClientWithAutoRedirects = factory.CreateClient();
        }

        [Fact]
        public async Task ProviderInfoNotBound_ReturnsSelectProviderView()
        {
            // Arrange
            await User.AsDeveloper();

            // Act
            var response = await _httpClientWithAutoRedirects.GetAsync("RedirectToProviderSelectionActionFilterTest");

            // Assert
            response.EnsureSuccessStatusCode();
            var textResponse = await response.Content.ReadAsStringAsync();
            Assert.Equal("Select Provider", textResponse);
        }

        [Fact]
        public async Task ActionDecoratedWithRequireProviderContext_ReturnsSelectProviderView()
        {
            // Arrange
            await User.AsDeveloper();

            // Act
            var response = await _httpClientWithAutoRedirects.GetAsync(
                "RedirectToProviderSelectionActionFilterTest/without-parameter");

            // Assert
            response.EnsureSuccessStatusCode();
            var textResponse = await response.Content.ReadAsStringAsync();
            Assert.Equal("Select Provider", textResponse);
        }
    }

    public class RedirectToProviderSelectionActionFilterTestController : Controller
    {
        [HttpGet("RedirectToProviderSelectionActionFilterTest")]
        public IActionResult Get(ProviderInfo providerInfo) => Ok("Yay");

        [HttpGet("RedirectToProviderSelectionActionFilterTest/without-parameter")]
        [RequireProviderContext]
        public IActionResult GetWithoutParameter() => Ok("Yay");
    }
}
