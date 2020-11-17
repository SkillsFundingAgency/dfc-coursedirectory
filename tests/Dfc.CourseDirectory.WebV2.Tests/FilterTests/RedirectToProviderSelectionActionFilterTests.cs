using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Testing;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FilterTests
{
    public class RedirectToProviderSelectionActionFilterTests : MvcTestBase
    {
        public RedirectToProviderSelectionActionFilterTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task ProviderInfoNotBound_ReturnsRedirectToSelectProviderView()
        {
            // Arrange
            await User.AsDeveloper();

            // Act
            var response = await HttpClient.GetAsync("RedirectToProviderSelectionActionFilterTest");

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/SearchProvider", UrlHelper.StripQueryParams(response.Headers.Location.OriginalString));
        }

        [Fact]
        public async Task ActionDecoratedWithRequireProviderContext_ReturnsRedirectToSelectProviderView()
        {
            // Arrange
            await User.AsDeveloper();

            // Act
            var response = await HttpClient.GetAsync(
                "RedirectToProviderSelectionActionFilterTest/without-parameter");

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/SearchProvider", UrlHelper.StripQueryParams(response.Headers.Location.OriginalString));
        }
    }

    public class RedirectToProviderSelectionActionFilterTestController : Controller
    {
        [HttpGet("RedirectToProviderSelectionActionFilterTest")]
        public IActionResult Get(ProviderContext providerContext) => Ok("Yay");

        [HttpGet("RedirectToProviderSelectionActionFilterTest/without-parameter")]
        [RequireProviderContext]
        public IActionResult GetWithoutParameter() => Ok("Yay");
    }
}
