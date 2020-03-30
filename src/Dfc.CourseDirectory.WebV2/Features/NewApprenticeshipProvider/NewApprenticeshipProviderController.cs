using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Flurl;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    [Route("new-apprenticeship-provider")]
    [RequireFeatureFlag(FeatureFlags.ApprenticeshipQA)]
    public class NewApprenticeshipProviderController :
        Controller,
        IMptxController<FlowModel>,
        IRequiresProviderContextController
    {
        private const string FlowName = "NewApprenticeshipProvider";

        private readonly IMediator _mediator;

        public NewApprenticeshipProviderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public MptxInstanceContext<FlowModel> Flow { get; set; }

        public ProviderInfo ProviderContext { get; set; }

        [MptxAction(FlowName)]
        [HttpGet("apprenticeship-details")]
        public async Task<IActionResult> ApprenticeshipDetails(StandardOrFramework standardOrFramework)
        {
            Flow.Update(s => s.SetApprenticeshipStandardOrFramework(standardOrFramework));
            var query = new ApprenticeshipDetails.Query() { ProviderId = ProviderContext.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [MptxAction(FlowName)]
        [HttpPost("apprenticeship-details")]
        public async Task<IActionResult> ApprenticeshipDetails(ApprenticeshipDetails.Command command)
        {
            command.ProviderId = ProviderContext.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(ApprenticeshipLocations))
                        .WithProviderContext(ProviderContext)
                        .WithMptxInstanceId(Flow.InstanceId)));
        }

        [HttpGet("apprenticeship-locations")]
        public IActionResult ApprenticeshipLocations() => throw new System.NotImplementedException();

        [StartsMptx]
        [HttpGet("provider-detail")]
        public async Task<IActionResult> ProviderDetail(
            [FromServices] MptxManager mptxManager,
            [FromServices] FlowModelInitializer initializer)
        {
            var flowModel = await initializer.Initialize(ProviderContext.ProviderId);
            var flow = mptxManager.CreateInstance(FlowName, flowModel);
            return RedirectToAction(nameof(ProviderDetail))
                .WithMptxInstanceId(flow);
        }

        [MptxAction(FlowName)]
        [HttpGet("provider-detail")]
        public async Task<IActionResult> ProviderDetail()
        {
            var query = new ProviderDetail.Query() { ProviderId = ProviderContext.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [MptxAction(FlowName)]
        [HttpPost("provider-detail")]
        public async Task<IActionResult> ProviderDetail(ProviderDetail.Command command)
        {
            command.ProviderId = ProviderContext.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(ProviderDetailConfirmation))
                        .WithProviderContext(ProviderContext)
                        .WithMptxInstanceId(Flow)));
        }

        [MptxAction(FlowName)]
        [HttpGet("provider-detail-confirmation")]
        public async Task<IActionResult> ProviderDetailConfirmation()
        {
            var query = new ProviderDetail.ConfirmationQuery() { ProviderId = ProviderContext.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [MptxAction(FlowName)]
        [HttpPost("provider-detail-confirmation")]
        public async Task<IActionResult> ProviderDetailConfirmation(ProviderDetail.ConfirmationCommand command)
        {
            command.ProviderId = ProviderContext.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                success => RedirectToAction(
                    "FindStandardOrFramework",
                    "Apprenticeships",
                    new
                    {
                        returnUrl = new Url(Url.Action(nameof(ApprenticeshipDetails)))
                            .WithProviderContext(ProviderContext)
                            .WithMptxInstanceId(Flow)
                    }));
        }
    }
}
