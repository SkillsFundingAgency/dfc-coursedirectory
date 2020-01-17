using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.Features
{
    public class ErrorTests : TestBase
    {
        public ErrorTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
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
    }

    public class ErrorTestController : Controller
    {
        [HttpGet("errortests")]
        public IActionResult Get() => throw new Exception("Bang!");
    }
}
