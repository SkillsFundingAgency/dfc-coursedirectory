using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FilterTests
{
    public class DeactivatedProviderErrorActionFilterTests : MvcTestBase
    {
        public DeactivatedProviderErrorActionFilterTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [MemberData(nameof(NotPermittedProviderTypes))]
        public async Task ProviderStatusNotPermitted_ReturnsProviderDeactivatedView(string providerStatus)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(ukprn: 12345);

            await User.AsProviderUser(providerId, Models.ProviderType.Both, providerStatus);

            // Act
            var response = await HttpClient.GetAsync("deactivatedprovidererroractionfiltertests/deactivated-not-allowed");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Deactivated provider", doc.QuerySelector("h1").TextContent);
        }

        [Theory]
        [MemberData(nameof(PermittedProviderTypes))]
        public async Task ProviderStatusPermitted_ReturnsOk(string providerStatus)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(ukprn: 12345);

            await User.AsProviderUser(providerId, Models.ProviderType.Both, providerStatus);

            // Act
            var response = await HttpClient.GetAsync("deactivatedprovidererroractionfiltertests/deactivated-not-allowed");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(NotPermittedProviderTypes))]
        public async Task ProviderStatusIgnored_ReturnsOk(string providerStatus)
        {
            // Arrange
            var providerId = await TestData.CreateProvider(ukprn: 12345);

            await User.AsProviderUser(providerId, Models.ProviderType.Both, providerStatus);

            // Act
            var response = await HttpClient.GetAsync("deactivatedprovidererroractionfiltertests/deactivated-allowed");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public static IEnumerable<object[]> PermittedProviderTypes { get; } = new[]
        {
            new[] { "Active" },
            new[] { "Verified" }
        };

        public static IEnumerable<object[]> NotPermittedProviderTypes { get; } = new[]
        {
            new[] { "Deactivation in process" },
            new[] { "Deactivation complete" },
        };
    }

    [Route("deactivatedprovidererroractionfiltertests")]
    public class DeactivatedProviderErrorActionFilterTestsController : Controller
    {
        [HttpGet("deactivated-allowed")]
        [AllowDeactivatedProvider]
        public IActionResult DeactivatedAllowed() => Ok();

        [HttpGet("deactivated-not-allowed")]
        public IActionResult DeactivatedNotAllowed() => Ok();
    }
}
