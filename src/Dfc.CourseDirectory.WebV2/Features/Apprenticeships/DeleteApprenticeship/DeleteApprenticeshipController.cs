using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Apprenticeships.DeleteApprenticeship
{  

    [Route("apprenticeships/delete/{ApprenticeshipId}")]
    [JourneyMetadata(
       journeyName: "DeleteApprenticeship",
       stateType: typeof(JourneyModel),
       appendUniqueKey: false,
       requestDataKeys: new[] { "ApprenticeshipId" })]
    public class DeleteApprenticeshipController : Controller
    {
        private readonly IMediator _mediator;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;
        private JourneyInstance _journeyInstance;

        public DeleteApprenticeshipController(
            IMediator mediator,
            JourneyInstanceProvider journeyInstanceProvider)
        {
            _mediator = mediator;
            _journeyInstanceProvider = journeyInstanceProvider;
            _journeyInstance = journeyInstanceProvider.GetInstance();
        }

        [HttpGet("")]
        [AuthorizeApprenticeship]
        public async Task<IActionResult> DeleteApprenticeship(Request request)
        {
            _journeyInstance = _journeyInstanceProvider.GetOrCreateInstance(() => new JourneyModel());

            return await _mediator.SendAndMapResponse(
                request,
                vm => View(vm));
        }

        [HttpPost("")]
        [AuthorizeApprenticeship]
        [RequireJourneyInstance]
        public Task<IActionResult> Post(
            Guid ApprenticeshipId,
            [FromServices] IProviderContextProvider providerContextProvider,
            Command request) =>
                _mediator.SendAndMapResponse(
                    request,
                    response => response.Match<IActionResult>(
                        errors => this.ViewFromErrors(errors),
                        vm => RedirectToAction(nameof(Deleted), new { ApprenticeshipId })
                            .WithProviderContext(providerContextProvider.GetProviderContext(true))));

        [HttpGet("success")]
        [RequireJourneyInstance]
        [RequireProviderContext]
        public async Task<IActionResult> Deleted(DeletedQuery request) =>
            await _mediator.SendAndMapResponse(request, vm => View(vm));
    }
}

