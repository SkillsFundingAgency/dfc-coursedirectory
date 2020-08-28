using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features;
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

        [Fact]
        public async Task ControllerImplementingIRequiresProviderContextControllerWithNoContext_ReturnsRedirectToSelectProviderView()
        {
            // Arrange
            await User.AsDeveloper();

            // Act
            var response = await HttpClient.GetAsync(
                "RedirectToProviderSelectionActionFilterTest2");

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/SearchProvider", UrlHelper.StripQueryParams(response.Headers.Location.OriginalString));
        }

        [Fact]
        public async Task ControllerImplementingIRequiresProviderContextController_HasProviderContextInjected()
        {
            // Arrange
            var providerId = await TestData.CreateProvider();

            await User.AsDeveloper();

            // Act
            var response = await HttpClient.GetAsync(
                $"RedirectToProviderSelectionActionFilterTest2?providerId={providerId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var textResponse = await response.Content.ReadAsStringAsync();
            Assert.Equal(providerId.ToString(), textResponse);
        }
    }

    public class RedirectToProviderSelectionActionFilterTestController : Controller
    {
        [HttpGet("RedirectToProviderSelectionActionFilterTest")]
        public IActionResult Get(ProviderInfo providerInfo) => Ok("Yay");

        [HttpGet("RedirectToProviderSelectionActionFilterTest/without-parameter")]
        [RequiresProviderContext]
        public IActionResult GetWithoutParameter() => Ok("Yay");
    }

    public class RedirectToProviderSelectionActionFilterTestController2 : Controller, IRequiresProviderContextController
    {
        public ProviderInfo ProviderContext { get; set; }

        [HttpGet("RedirectToProviderSelectionActionFilterTest2")]
        public IActionResult Get() => Content(ProviderContext.ProviderId.ToString());
    }
}
