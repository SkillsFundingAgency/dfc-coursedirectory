using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
            var providerId = Guid.NewGuid();
            var ukprn = 12345;

            Factory.ProviderInfoCache
                .Setup(mock => mock.GetProviderInfo(ukprn))
                .ReturnsAsync(new ProviderInfo()
                {
                    ProviderId = providerId,
                    Ukprn = ukprn
                });

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
