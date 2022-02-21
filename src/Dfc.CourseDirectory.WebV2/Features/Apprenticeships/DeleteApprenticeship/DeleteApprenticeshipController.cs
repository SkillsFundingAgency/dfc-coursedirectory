using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;


namespace Dfc.CourseDirectory.WebV2.Features.Apprenticeships.DeleteApprenticeship
{
    [JourneyMetadata(
        journeyName: nameof(DeleteApprenticeship),
        stateType: typeof(JourneyModel),
        appendUniqueKey: false,
        requestDataKeys: new[] { nameof(Command.ApprenticeshipId) })]
    [RequireProviderContext]
    [Route("apprenticeships/delete/{ApprenticeshipId}")]
    public class DeleteApprenticeshipController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IProviderContextProvider _providerContextProvider;

        public DeleteApprenticeshipController(IMediator mediator, IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _providerContextProvider = providerContextProvider;
        }


        [HttpGet("")]
        public async Task<IActionResult> DeleteApprenticeship(Guid ApprenticeshipId)
        {
            var quck = _providerContextProvider.GetProviderContext(true).ProviderInfo.ProviderId;

            return await _mediator.SendAndMapResponse(
                new Request
                {
                    ApprenticeshipId = ApprenticeshipId
                },
                vm => View(vm));
        }


        
        [HttpPost("")]
        public async Task<IActionResult> DeleteApprenticeship(Command request) => await _mediator.SendAndMapResponse(
            request,
            r => r.Match<IActionResult>(
                errors => this.ViewFromErrors(errors),
                _ => RedirectToAction(nameof(DeleteApprenticeship), new { request.ApprenticeshipId})
                    .WithProviderContext(_providerContextProvider.GetProviderContext())));
        
       [RequireJourneyInstance]
       [HttpGet("delete-success")]
          public Task<IActionResult> DeletedApprenticeship() =>
            _mediator.SendAndMapResponse<DeletedViewModel, IActionResult>(new DeletedQuery(), vm => View(vm));
    } 
}

