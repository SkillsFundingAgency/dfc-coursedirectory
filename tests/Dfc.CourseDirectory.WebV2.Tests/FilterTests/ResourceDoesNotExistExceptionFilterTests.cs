using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FilterTests
{
    public class ResourceDoesNotExistExceptionFilterTests : MvcTestBase
    {
        public ResourceDoesNotExistExceptionFilterTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task ActionThrowsResourceDoesNotExistException_ReturnsNotFoundStatusCode()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "ResourceDoesNotExistExceptionFilterTests");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    [Route("ResourceDoesNotExistExceptionFilterTests")]
    public class ResourceDoesNotExistExceptionFilterTestsController : Controller
    {
        [HttpGet("")]
        public IActionResult Get() => throw new ResourceDoesNotExistException(
            resourceType: ResourceType.Course, resourceId: Guid.NewGuid());
    }
}
