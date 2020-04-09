using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.MultiPageTransaction
{
    public class ActionResolutionTests : MvcTestBase
    {
        public ActionResolutionTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task NoInstanceIdInRequest_ChoosesActionWithStartsMptxAttribute()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync($"ActionResolutionTests");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Starts", content);
        }

        [Fact]
        public async Task InstanceIdInRequest_ChoosesActionWithMptxActionAttribute()
        {
            // Arrange
            var instance = MptxManager.CreateInstance(new ActionResolutionTestsFlowState());

            // Act
            var response = await HttpClient.GetAsync($"ActionResolutionTests?ffiid={instance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Get", content);
        }
    }

    [Route("ActionResolutionTests")]
    public class ActionResolutionTestsController : Controller
    {
        [StartsMptx]
        [HttpGet("")]
        public IActionResult Starts() => Content(nameof(Starts));

        [MptxAction]
        [HttpGet("")]
        public IActionResult Get() => Content(nameof(Get));
    }

    public class ActionResolutionTestsFlowState : IMptxState
    {
    }
}
