using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class MultiPageTransactionTests : MvcTestBase
    {
        public MultiPageTransactionTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task RequestWithNoInstanceIdToStartsMptxAction_CreatesInstanceAndRedirects()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("MultiPageTransactionTests/starts");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.StartsWith(
                "/MultiPageTransactionTests/first?ffiid=",
                response.Headers.Location.OriginalString);

            var state = Assert.IsType<MultiPageTransactionTestsFlowState>(
                Factory.MptxStateProvider.Instances.Single().Value.State);
            Assert.NotNull(state);
        }

        [Fact]
        public async Task RequestWithNoInstanceId_ReturnsNotFound()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("MultiPageTransactionTests/second");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ModelBindingMptxInstanceContext_Succeeds()
        {
            // Arrange
            var instance = Factory.MptxStateProvider.CreateInstance(
                typeof(MultiPageTransactionTestsFlowState),
                new Dictionary<string, object>(),
                new MultiPageTransactionTestsFlowState());

            // Act
            var response = await HttpClient.GetAsync(
                $"MultiPageTransactionTests/modelbinds?ffiid={instance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal(instance.InstanceId, responseContent);
        }

        [Fact]
        public async Task ModelBindingMptxInstanceContextMismatchedType_ReturnsBadRequest()
        {
            // Arrange
            var instance = Factory.MptxStateProvider.CreateInstance(
                typeof(MultiPageTransactionTestsFlowState),
                new Dictionary<string, object>(),
                state: new MultiPageTransactionTestsFlowState());

            // Act
            var response = await HttpClient.GetAsync(
                $"MultiPageTransactionTests/modelbindswrongtype?ffiid={instance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task StateUpdates_PersistedToStore()
        {
            // Arrange
            var instance = Factory.MptxStateProvider.CreateInstance(
                typeof(MultiPageTransactionTestsFlowState),
                new Dictionary<string, object>(),
                new MultiPageTransactionTestsFlowState());

            // Act
            var response = await HttpClient.GetAsync(
                $"MultiPageTransactionTests/updatesstate?ffiid={instance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var state = Assert.IsType<MultiPageTransactionTestsFlowState>(
                Factory.MptxStateProvider.Instances.Single().Value.State);
            Assert.Equal(69, state.Bar);
        }

        [Fact]
        public async Task CompleteInstance_RemovesInstanceFromStore()
        {
            // Arrange
            var instance = Factory.MptxStateProvider.CreateInstance(
                typeof(MultiPageTransactionTestsFlowState),
                new Dictionary<string, object>(),
                new MultiPageTransactionTestsFlowState());

            // Act
            var response = await HttpClient.GetAsync(
                $"MultiPageTransactionTests/final?ffiid={instance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.DoesNotContain(instance.InstanceId, Factory.MptxStateProvider.Instances.Keys);
        }
    }

    public class MultiPageTransactionTestsFlowState : IMptxState
    {
        public int Foo { get; set; }
        public int Bar { get; set; }
        public int Baz { get; set; }
    }

    public class MultiPageTransactionTestsDifferentFlowState : IMptxState
    {
    }

    [Route("MultiPageTransactionTests")]
    public class MultiPageTransactionTestsController : Controller, IMptxController<MultiPageTransactionTestsFlowState>
    {
        public MptxInstanceContext<MultiPageTransactionTestsFlowState> Flow { get; set; }

        [StartsMptx]
        [HttpGet("starts")]
        public IActionResult Starts([FromServices] MptxManager mptxManager)
        {
            var flow = mptxManager.CreateInstance(typeof(MultiPageTransactionTestsFlowState));
            return RedirectToAction(nameof(First)).WithMptxInstanceId(flow);
        }

        [MptxAction]
        [HttpGet("first")]
        public IActionResult First() => Ok();

        [MptxAction]
        [HttpGet("second")]
        public IActionResult Second() => Ok();

        [MptxAction]
        [HttpGet("modelbinds")]
        public IActionResult ModelBinds(MptxInstanceContext<MultiPageTransactionTestsFlowState> flowState)
        {
            flowState.Update(s => s.Foo = 42);
            return Ok(flowState.InstanceId);
        }

        [MptxAction]
        [HttpGet("modelbindswrongtype")]
        public IActionResult ModelBindsWrongType(
            MptxInstanceContext<MultiPageTransactionTestsDifferentFlowState> flowState) => Ok();

        [MptxAction]
        [HttpGet("updatesstate")]
        public IActionResult UpdatesState(
            MptxInstanceContext<MultiPageTransactionTestsFlowState> flowState)
        {
            flowState.Update(s => s.Bar = 69);
            return Ok();
        }

        [MptxAction]
        [HttpGet("final")]
        public IActionResult Final(MptxInstanceContext<MultiPageTransactionTestsFlowState> flowState)
        {
            flowState.Complete();
            return Ok();
        }
    }
}
