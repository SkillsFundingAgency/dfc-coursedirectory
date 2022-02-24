using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;


namespace Dfc.CourseDirectory.WebV2.Features.Apprenticeships.DeleteApprenticeship
{  /*
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

        /*
        [HttpGet("")]
        public async Task<IActionResult> DeleteApprenticeship(Guid ApprenticeshipId)
        {
            var query = _providerContextProvider.GetProviderContext(true).ProviderInfo.ProviderId;

            return await _mediator.SendAndMapResponse(
                new Request
                {
                    ApprenticeshipId = ApprenticeshipId
                },
                vm => View(vm));
        } */
    /*
       [HttpGet("")]
       public async Task<IActionResult> DeleteApprenticeship(Guid ApprenticeshipId) =>
         await _mediator.SendAndMapResponse(
             new Query
             {
                 ApprenticeshipId = ApprenticeshipId,
                 ProviderId = _providerContextProvider.GetProviderContext().ProviderInfo.ProviderId
             },
             vm => View(vm));



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
   }  */

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
        //  [AuthorizeApprenticeship]
        public async Task<IActionResult> DeleteApprenticeship(Request request)
        {
            _journeyInstance = _journeyInstanceProvider.GetOrCreateInstance(() => new JourneyModel());

            return await _mediator.SendAndMapResponse(
                request,
                vm => View(vm));
        }

        [HttpPost("")]
       // [AuthorizeTLevel]
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

