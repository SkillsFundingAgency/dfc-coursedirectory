using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.DeleteTLevel
{
    [Route("t-levels/{tLevelId}/delete")]
    [JourneyMetadata(
        journeyName: "DeleteTLevel",
        stateType: typeof(JourneyModel),
        appendUniqueKey: false,
        requestDataKeys: new[] { "tLevelId" })]
    public class DeleteTLevelController : Controller
    {
        private readonly IMediator _mediator;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;
        private JourneyInstance _journeyInstance;

        public DeleteTLevelController(
            IMediator mediator,
            JourneyInstanceProvider journeyInstanceProvider)
        {
            _mediator = mediator;
            _journeyInstanceProvider = journeyInstanceProvider;
            _journeyInstance = journeyInstanceProvider.GetInstance();
        }

        [HttpGet("")]
        [AuthorizeTLevel]
        public async Task<IActionResult> Get(Request request)
        {
            _journeyInstance = _journeyInstanceProvider.GetOrCreateInstance(() => new JourneyModel());

            return await _mediator.SendAndMapResponse(
                request,
                vm => View(vm));
        }

        [HttpPost("")]
        [AuthorizeTLevel]
        [RequireJourneyInstance]
        public Task<IActionResult> Post(
            Guid tLevelId,
            [FromServices] IProviderContextProvider providerContextProvider,
            Command request) =>
                _mediator.SendAndMapResponse(
                    request,
                    response => response.Match<IActionResult>(
                        errors => this.ViewFromErrors(errors),
                        vm => RedirectToAction(nameof(Deleted), new { tLevelId })
                            .WithProviderContext(providerContextProvider.GetProviderContext())));

        [HttpGet("success")]
        [RequireJourneyInstance]
        [RequireProviderContext]
        public async Task<IActionResult> Deleted(DeletedQuery request) =>
            await _mediator.SendAndMapResponse(request, vm => View(vm));
    }
}
