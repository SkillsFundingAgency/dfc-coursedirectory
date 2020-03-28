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
    public class NewApprenticeshipProviderController : Controller, IRequiresProviderContextController
    {
        private const string FlowName = "NewApprenticeshipProvider";

        private readonly IMediator _mediator;

        public NewApprenticeshipProviderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public ProviderInfo ProviderContext { get; set; }

        [MptxAction(FlowName)]
        [HttpGet("apprenticeship-details")]
        public async Task<IActionResult> ApprenticeshipDetails(
            StandardOrFramework standardOrFramework,
            MptxInstanceContext<FlowModel> flow)
        {
            flow.Update(s => s.SetApprenticeshipStandardOrFramework(standardOrFramework));
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
                    success => RedirectToAction("ApprenticeshipLocations").WithProviderContext(ProviderContext)));
        }

        [HttpGet("apprenticeship-locations")]
        public IActionResult ApprenticeshipLocations() => throw new System.NotImplementedException();

        [StartsMptx]
        public async Task<IActionResult> ProviderDetail(
            ProviderInfo providerInfo,
            [FromServices] MptxManager mptxManager,
            [FromServices] FlowModelInitializer initializer)
        {
            var flowModel = await initializer.Initialize(providerInfo.ProviderId);
            var flow = mptxManager.CreateInstance(FlowName, flowModel);
            return RedirectToAction(nameof(ProviderDetail))
                .WithMptxInstanceId(flow);
        }

        [MptxAction(FlowName)]
        [HttpGet("provider-detail")]
        public async Task<IActionResult> ProviderDetail(ProviderInfo providerInfo)
        {
            var query = new ProviderDetail.Query() { ProviderId = providerInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [MptxAction(FlowName)]
        [HttpPost("provider-detail")]
        public async Task<IActionResult> ProviderDetail(
            ProviderInfo providerInfo,
            MptxInstanceContext<FlowModel> flow,
            ProviderDetail.Command command)
        {
            command.ProviderId = providerInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(ProviderDetailConfirmation))
                        .WithProviderContext(providerInfo)
                        .WithMptxInstanceId(flow)));
        }

        [MptxAction(FlowName)]
        [HttpGet("provider-detail-confirmation")]
        public async Task<IActionResult> ProviderDetailConfirmation(ProviderInfo providerInfo)
        {
            var query = new ProviderDetail.ConfirmationQuery() { ProviderId = providerInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [MptxAction(FlowName)]
        [HttpPost("provider-detail-confirmation")]
        public async Task<IActionResult> ProviderDetailConfirmation(
            ProviderInfo providerInfo,
            MptxInstanceContext<FlowModel> flow,
            ProviderDetail.ConfirmationCommand command)
        {
            command.ProviderId = providerInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                success => RedirectToAction(
                    "FindStandardOrFramework",
                    "Apprenticeships",
                    new
                    {
                        returnUrl = new Url(Url.Action("ApprenticeshipDetails"))
                            .WithProviderContext(providerInfo)
                            .WithMptxInstanceId(flow)
                    }));
        }
    }
}
