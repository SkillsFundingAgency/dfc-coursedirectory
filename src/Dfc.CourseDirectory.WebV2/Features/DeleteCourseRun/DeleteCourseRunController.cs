using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.DeleteCourseRun
{
    [Route("courses/{courseId}/course-runs/{courseRunId}/delete")]
    [FormFlowAction("DeleteCourseRun", typeof(FlowModel), idRouteParameterNames: new[] { "courseId", "courseRunId" })]
    public class DeleteCourseRunController : Controller
    {
        private readonly IMediator _mediator;

        public DeleteCourseRunController(IMediator mediator) => _mediator = mediator;

        [HttpGet("")]
        [AuthorizeCourse]
        public async Task<IActionResult> Get(
            Request request,
            FormFlowInstanceFactory instanceFactory,
            [LocalUrl(viewDataKey: "ReturnUrl")] string returnUrl)
        {
            instanceFactory.GetOrCreateInstance(() => new FlowModel());

            return await _mediator.SendAndMapResponse(
                request,
                vm => View(vm));
        }

        [HttpPost("")]
        [AuthorizeCourse]
        public Task<IActionResult> Post(Command request, FormFlowInstance instance) =>
            _mediator.SendAndMapResponse(
                request,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    vm => RedirectToAction(nameof(Confirmed))
                        .WithFormFlowInstanceId(instance)));

        [HttpGet("confirmed")]
        public async Task<IActionResult> Confirmed(
            [FromServices] IProviderContextProvider providerContextProvider,
            [FromServices] IProviderInfoCache providerInfoCache,
            FormFlowInstance instance,
            ConfirmedQuery request)
        {
            var vm = await _mediator.Send(request);

            var providerInfo = await providerInfoCache.GetProviderInfo(vm.ProviderId);
            providerContextProvider.SetProviderContext(new ProviderContext(providerInfo));

            return View(vm);
        }
    }
}
