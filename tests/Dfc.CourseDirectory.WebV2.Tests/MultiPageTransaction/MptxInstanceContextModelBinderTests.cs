using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public async Task InstanceContext_BindsSuccessfully()
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

        [Fact]
        public async Task InstanceContextForDerivedType_BindsSuccessfully()
        {
            // Arrange
            var instance = MptxManager.CreateInstance(new MptxInstanceContextModelBinderTestsFlowState() { Foo = 42 });

            // Act
            var response = await HttpClient.GetAsync(
                $"MptxInstanceContextModelBinderTests/derived?ffiid={instance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("42", content);
        }

        [Fact]
        public async Task InstanceContextWithParent_BindsSuccessfully()
        {
            // Arrange
            var parentInstance = MptxManager.CreateInstance(
                new MptxInstanceContextModelBinderTestsFlowState() { Foo = 42 });

            var instance = MptxManager.CreateInstance(
                parentInstance,
                new MptxInstanceContextModelBinderTestsChildFlowState() { Bar = 69 });

            // Act
            var response = await HttpClient.GetAsync(
                $"MptxInstanceContextModelBinderTests/child?ffiid={instance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var contentJson = JObject.Parse(content);
            Assert.Equal(42, contentJson["foo"]);
            Assert.Equal(69, contentJson["bar"]);
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

        [MptxAction]
        [HttpGet("derived")]
        public IActionResult Derived(MptxInstanceContext<IMptxInstanceContextModelBinderTestsFlowState> flow) =>
            Content(flow.State.Foo.ToString());

        [MptxAction]
        [HttpGet("child")]
        public IActionResult Child(MptxInstanceContext<MptxInstanceContextModelBinderTestsChildFlowState, MptxInstanceContextModelBinderTestsFlowState> flow) =>
            Json(new
            {
                flow.ParentState.Foo,
                flow.State.Bar
            });
    }

    public interface IMptxInstanceContextModelBinderTestsFlowState : IMptxState
    {
        int Foo { get; set; }
    }

    public class MptxInstanceContextModelBinderTestsFlowState : IMptxInstanceContextModelBinderTestsFlowState
    {
        public int Foo { get; set; }
    }

    public class MptxInstanceContextModelBinderTestsDifferentFlowState : IMptxState
    {
    }

    public class MptxInstanceContextModelBinderTestsChildFlowState : IMptxState<MptxInstanceContextModelBinderTestsFlowState>
    {
        public int Bar { get; set; }
    }
}
