using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.DeleteCourseRun
{
    [Route("courses/{courseId}/course-runs/{courseRunId}/delete")]
    [JourneyMetadata(
        journeyName: "DeleteCourseRun",
        stateType: typeof(JourneyModel),
        appendUniqueKey: false,
        requestDataKeys: new[] { "courseId", "courseRunId" })]
    public class DeleteCourseRunController : Controller
    {
        private readonly IMediator _mediator;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;
        private JourneyInstance _journeyInstance;

        public DeleteCourseRunController(
            IMediator mediator,
            JourneyInstanceProvider journeyInstanceProvider)
        {
            _mediator = mediator;
            _journeyInstanceProvider = journeyInstanceProvider;
            _journeyInstance = journeyInstanceProvider.GetInstance();
        }

        [HttpGet("")]
        [AuthorizeCourse]
        public async Task<IActionResult> Get(
            Request request,
            [LocalUrl(viewDataKey: "ReturnUrl")] string returnUrl)
        {
            _journeyInstance = _journeyInstanceProvider.GetOrCreateInstance(() => new JourneyModel());

            return await _mediator.SendAndMapResponse(
                request,
                vm => View(vm));
        }

        [HttpPost("")]
        [AuthorizeCourse]
        [RequireJourneyInstance]
        public Task<IActionResult> Post(Guid courseId, Guid courseRunId, Command request) =>
            _mediator.SendAndMapResponse(
                request,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    vm => RedirectToAction(
                        nameof(Confirmed),
                        new { courseId, courseRunId })));

        [HttpGet("confirmed")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Confirmed(
            [FromServices] IProviderContextProvider providerContextProvider,
            [FromServices] IProviderInfoCache providerInfoCache,
            ConfirmedQuery request)
        {
            var vm = await _mediator.Send(request);

            var providerInfo = await providerInfoCache.GetProviderInfo(vm.ProviderId);
            providerContextProvider.SetProviderContext(new ProviderContext(providerInfo));

            return View(vm);
        }
    }
}
