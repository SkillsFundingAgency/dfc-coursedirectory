using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests
{
    public class ErrorTests : TestBase
    {
        public ErrorTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
            Factory.HostingOptions.RewriteForbiddenToNotFound = true;
        }

        [Fact]
        public async Task UnhandledException_ReturnsErrorView()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("errortests");

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Sorry, there is a problem with the service", doc.QuerySelector("h1").TextContent);
        }

        [Fact]
        public async Task RequestingErrorUrlDirectly_ReturnsNotFound()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("error");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task NotFound_ReturnsNotFoundView()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("notarealurl");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Page not found", doc.QuerySelector("h1").TextContent);
        }

        [Fact]
        public async Task Forbidden_ReturnsNotFoundView()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("errortests/forbidden");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Page not found", doc.QuerySelector("h1").TextContent);
        }
    }

    public class ErrorTestController : Controller
    {
        [HttpGet("errortests")]
        public IActionResult Get() => throw new Exception("Bang!");

        [HttpGet("errortests/forbidden")]
        public IActionResult Forbidden() => Forbid();
    }
}
