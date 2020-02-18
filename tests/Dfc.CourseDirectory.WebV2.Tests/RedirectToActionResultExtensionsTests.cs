using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class RedirectToActionResultExtensionsTests : TestBase
    {
        public RedirectToActionResultExtensionsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task WithCurrentProvider_AppendsUkprnToRouteValues()
        {
            // Arrange
            var ukprn = 12345;
            var providerId = await TestData.CreateProvider(ukprn);
            User.AsDeveloper();  // Ensure ukprn is bound from query param

            var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions()
            {
                AllowAutoRedirect = false
            });

            // Act
            var response = await client.GetAsync($"redirecttoactionresultextensionstestcontroller/first?ukprn={ukprn}");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            var location = response.Headers.Location;
            Assert.Equal($"/redirecttoactionresultextensionstestcontroller/second?ukprn={ukprn}", location.OriginalString);
        }
    }

    public class RedirectToActionResultExtensionsTestController : Controller
    {
        [HttpGet("redirecttoactionresultextensionstestcontroller/first")]
        public IActionResult First(ProviderInfo providerInfo) => RedirectToAction("Second").WithCurrentProvider(providerInfo);

        [HttpGet("redirecttoactionresultextensionstestcontroller/second")]
        public IActionResult Second(ProviderInfo providerInfo) => Json(providerInfo);
    }
}
