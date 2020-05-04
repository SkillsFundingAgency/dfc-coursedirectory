using System;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.MultiPageTransaction
{
    public class MptxResourceFilterTests : MvcTestBase
    {
        public MptxResourceFilterTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task ActionDecoratedWithMptxActionAttributeAndValidInstanceSpecified_AssignsMptxInstanceContextFeature()
        {
            // Arrange
            var instance = MptxManager.CreateInstance(new MptxResourceFilterTestsFlowState());

            // Act
            var response = await HttpClient.GetAsync($"MptxResourceFilterTests?ffiid={instance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(instance.InstanceId, content);
        }

        [Fact]
        public async Task ActionDecoratedWithMptxActionAttributeButNoValidInstanceSpecified_ReturnsErrorView()
        {
            // Arrange
            var invalidInstanceId = Guid.NewGuid();

            // Act
            var response = await HttpClient.GetAsync($"MptxResourceFilterTests?ffiid={invalidInstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("Sorry, there is a problem with the service", doc.QuerySelector("h1").TextContent);
        }
    }

    [Route("MptxResourceFilterTests")]
    public class MptxResourceFilterTestsController : Controller
    {
        [MptxAction]
        [HttpGet("")]
        public IActionResult Get()
        {
            var feature = HttpContext.Features.Get<MptxInstanceFeature>();
            return Content(feature.Instance.InstanceId);
        }
    }

    public class MptxResourceFilterTestsFlowState : IMptxState
    {
    }
}
