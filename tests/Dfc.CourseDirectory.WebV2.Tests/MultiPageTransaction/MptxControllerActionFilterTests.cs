using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.MultiPageTransaction
{
    public class MptxControllerActionFilterTests : MvcTestBase
    {
        public MptxControllerActionFilterTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task HaveInstance_SetsFlowProperty()
        {
            // Arrange
            var instance = MptxManager.CreateInstance(new MptxControllerActionFilterTestsFlowState() { Foo = 42 });

            // Act
            var response = await HttpClient.GetAsync(
                $"MptxControllerActionFilterTests?ffiid={instance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("42", content);
        }

        [Fact]
        public async Task IncorrectStateTypeParameter_ReturnsBadRequest()
        {
            // Arrange
            var instance = MptxManager.CreateInstance(new MptxControllerActionFilterTestsDifferentFlowState());

            // Act
            var response = await HttpClient.GetAsync(
                $"MptxControllerActionFilterTests?ffiid={instance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    [Route("MptxControllerActionFilterTests")]
    public class MptxControllerActionFilterTestsController :
        Controller,
        IMptxController<MptxControllerActionFilterTestsFlowState>
    {
        public MptxInstanceContext<MptxControllerActionFilterTestsFlowState> Flow { get; set; }

        [MptxAction]
        [HttpGet("")]
        public IActionResult Get() => Content(Flow.State.Foo.ToString());
    }

    public class MptxControllerActionFilterTestsFlowState : IMptxState
    {
        public int Foo { get; set; }
    }

    public class MptxControllerActionFilterTestsDifferentFlowState : IMptxState
    {
    }
}
