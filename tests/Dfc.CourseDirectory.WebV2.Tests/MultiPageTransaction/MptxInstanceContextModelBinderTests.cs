using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.MultiPageTransaction
{
    public class MptxInstanceContextModelBinderTests : MvcTestBase
    {
        public MptxInstanceContextModelBinderTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task InstanceContext_BoundSuccessfully()
        {
            // Arrange
            var instance = MptxManager.CreateInstance(new MptxInstanceContextModelBinderTestsFlowState() { Foo = 42 });

            // Act
            var response = await HttpClient.GetAsync(
                $"MptxInstanceContextModelBinderTests?ffiid={instance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("42", content);
        }

        [Fact]
        public async Task IncorrectStateTypeParameter_ReturnsBadRequest()
        {
            // Arrange
            var instance = MptxManager.CreateInstance(new MptxInstanceContextModelBinderTestsFlowState() { Foo = 42 });

            // Act
            var response = await HttpClient.GetAsync(
                $"MptxInstanceContextModelBinderTests/wrong-state-type?ffiid={instance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    [Route("MptxInstanceContextModelBinderTests")]
    public class MptxInstanceContextModelBinderTestsController : Controller
    {
        [MptxAction]
        [HttpGet("")]
        public IActionResult Get(MptxInstanceContext<MptxInstanceContextModelBinderTestsFlowState> flow) =>
            Content(flow.State.Foo.ToString());

        [MptxAction]
        [HttpGet("wrong-state-type")]
        public IActionResult WrongStateType(MptxInstanceContext<MptxInstanceContextModelBinderTestsDifferentFlowState> flow) =>
            Ok();
    }

    public class MptxInstanceContextModelBinderTestsFlowState : IMptxState
    {
        public int Foo { get; set; }
    }

    public class MptxInstanceContextModelBinderTestsDifferentFlowState : IMptxState
    {
    }
}
