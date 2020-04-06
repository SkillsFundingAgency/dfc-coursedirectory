using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FilterTests
{
    public class RequireFeatureFlagFilterAttributeTests : MvcTestBase
    {
        public RequireFeatureFlagFilterAttributeTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task FeatureNotConfigured_ReturnsNotFound()
        {
            // Arrange
            Factory.FeatureFlagProvider.SetFeatures();

            // Act
            var response = await HttpClient.GetAsync("requirefeatureflagfilterattributetests");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task FeatureIsConfigured_ReturnsOk()
        {
            // Arrange
            Factory.FeatureFlagProvider.SetFeatures("TestFeature");

            // Act
            var response = await HttpClient.GetAsync("requirefeatureflagfilterattributetests");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    public class RequireFeatureFlagFilterAttributeTestsController : Controller
    {
        [HttpGet("requirefeatureflagfilterattributetests")]
        [RequireFeatureFlag("TestFeature")]
        public IActionResult Get() => Ok();
    }
}
