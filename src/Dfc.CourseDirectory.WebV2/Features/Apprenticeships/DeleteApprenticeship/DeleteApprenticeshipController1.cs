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
    [Route("apprenticeships/{ApprenticeshipId}/delete")]
    public class DeleteApprenticeshipController1 : Controller
    {
        private readonly IMediator _mediator;
        private readonly IProviderContextProvider _providerContextProvider;

        public DeleteApprenticeshipController1(IMediator mediator, IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _providerContextProvider = providerContextProvider;
        }

        
        [HttpGet("delete")]
        public async Task<IActionResult> DeleteApprenticeship(Guid ApprenticeshipId) =>
            await _mediator.SendAndMapResponse(
                new Query
                {
                    ApprenticeshipId = ApprenticeshipId,
                    ProviderId = _providerContextProvider.GetProviderContext().ProviderInfo.ProviderId
                },
                vm => View(vm));

        
        [HttpPost("delete")]
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

