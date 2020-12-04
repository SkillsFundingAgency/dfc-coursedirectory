using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FilterTests
{
    public class StateExpiredExceptionFilterTests : MvcTestBase
    {
        public StateExpiredExceptionFilterTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task ActionThrowsResourceDoesNotExistException_ReturnsNotFoundStatusCode()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "StateExpiredExceptionFilterTests");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            var doc = await response.GetDocument();
            doc.GetElementsByTagName("html")[0].GetAttribute("data-viewpath").Should().Be(
                "/SharedViews/PageExpiredError.cshtml");
        }
    }

    [Route("StateExpiredExceptionFilterTests")]
    public class StateExpiredExceptionFilterTestsController : Controller
    {
        [HttpGet("")]
        public IActionResult Get() => throw new StateExpiredException();
    }
}
