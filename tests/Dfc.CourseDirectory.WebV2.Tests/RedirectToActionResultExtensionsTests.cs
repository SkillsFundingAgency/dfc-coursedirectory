﻿using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class RedirectToActionResultExtensionsTests : MvcTestBase
    {
        public RedirectToActionResultExtensionsTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task WithProviderContext_AppendsProviderIdToRouteValues()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsDeveloper();  // Ensure ukprn is bound from query param

            // Act
            var response = await HttpClient.GetAsync(
                $"redirecttoactionresultextensionstestcontroller/first?providerId={provider.ProviderId}");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            var location = response.Headers.Location;
            Assert.Equal($"/redirecttoactionresultextensionstestcontroller/second?providerId={provider.ProviderId}", location.OriginalString);
        }
    }

    public class RedirectToActionResultExtensionsTestController : Controller
    {
        [HttpGet("redirecttoactionresultextensionstestcontroller/first")]
        public IActionResult First([FromServices] IProviderContextProvider providerContextProvider) =>
            RedirectToAction("Second").WithProviderContext(providerContextProvider.GetProviderContext());

        [HttpGet("redirecttoactionresultextensionstestcontroller/second")]
        public IActionResult Second(ProviderContext providerContext) => Json(providerContext.ProviderInfo);
    }
}
