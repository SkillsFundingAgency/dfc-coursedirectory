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

        [HttpGet("apprenticeship-classroom-locations")]
        public IActionResult ApprenticeshipClassroomLocations() => throw new System.NotImplementedException();

        [HttpGet("apprenticeship-mixed-locations")]
        public IActionResult ApprenticeshipClassroomBasedAndEmployerBased() => throw new System.NotImplementedException();

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
                    success => RedirectToAction("ApprenticeshipLocations")
                        .WithProviderContext(ProviderContext)
                        .WithMptxInstanceId(Flow.InstanceId)));
        }

        [MptxAction(FlowName)]
        [HttpGet("apprenticeship-employer-locations")]
        public async Task<IActionResult> ApprenticeshipEmployerLocations()
        {
            var query = new ApprenticeshipEmployerLocations.Query() { ProviderId = ProviderContext.ProviderId };
            return await _mediator.SendAndMapResponse(
                query,
                response => View(response));
        }

        [MptxAction(FlowName)]
        [HttpPost("apprenticeship-employer-locations")]
        public async Task<IActionResult> ApprenticeshipEmployerLocations(ApprenticeshipEmployerLocations.Command command)
        {
            command.ProviderId = ProviderContext.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success =>
                        (command.National.Value ?
                            RedirectToAction(nameof(ApprenticeshipSummary)) :
                            RedirectToAction(nameof(ApprenticeshipEmployerLocationsRegions)))
                        .WithProviderContext(ProviderContext).WithMptxInstanceId(Flow.InstanceId)));
        }

        [MptxAction(FlowName)]
        [HttpGet("apprenticeship-employer-locations-regions")]
        public async Task<IActionResult> ApprenticeshipEmployerLocationsRegions()
        {
            var query = new ApprenticeshipEmployerLocationsRegions.Query() { ProviderId = ProviderContext.ProviderId };
            return await _mediator.SendAndMapResponse(query, command => View(command));
        }

        [MptxAction(FlowName)]
        [HttpPost("apprenticeship-employer-locations-regions")]
        public async Task<IActionResult> ApprenticeshipEmployerLocationsRegions(ApprenticeshipEmployerLocationsRegions.Command command)
        {
            command.ProviderId = ProviderContext.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success =>
                        (Flow.State.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased ?
                            RedirectToAction(nameof(ApprenticeshipSummary)) :
                            RedirectToAction(nameof(ApprenticeshipClassroomLocations)))
                                .WithProviderContext(ProviderContext)
                                .WithMptxInstanceId(Flow)));
        }

        [MptxAction(FlowName)]
        [HttpGet("apprenticeship-locations")]
        public async Task<IActionResult> ApprenticeshipLocations()
        {
            var query = new ApprenticeshipLocations.Query() { ProviderId = ProviderContext.ProviderId };
            return await _mediator.SendAndMapResponse(
                query,
                response => View(response));
        }

        [MptxAction(FlowName)]
        [HttpPost("apprenticeship-locations")]
        public async Task<IActionResult> ApprenticeshipLocations(ApprenticeshipLocations.Command command)
        {
            command.ProviderId = ProviderContext.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success =>
                        (command.LocationType.Value switch
                        {
                            ApprenticeshipLocationType.ClassroomBased => RedirectToAction(nameof(ApprenticeshipClassroomLocations)),
                            ApprenticeshipLocationType.EmployerBased => RedirectToAction(nameof(ApprenticeshipEmployerLocations)),
                            _ => RedirectToAction(nameof(ApprenticeshipClassroomBasedAndEmployerBased))
                        }).WithProviderContext(ProviderContext).WithMptxInstanceId(Flow.InstanceId)));
        }
        [MptxAction(FlowName)]
        [HttpGet("apprenticeship-confirmation")]
        public async Task<IActionResult> ApprenticeshipSummary()
        {
            var query = new ApprenticeshipSummary.Query() { ProviderId = ProviderContext.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [MptxAction(FlowName)]
        [HttpPost("apprenticeship-confirmation")]
        public async Task<IActionResult> ApprenticeshipSummaryConfirmation()
        {
            var command = new ApprenticeshipSummary.CompleteCommand() { ProviderId = ProviderContext.ProviderId };
            return await _mediator.SendAndMapResponse(
                command,
                success => View("Submitted"));
        }

        [StartsMptx]
        [HttpGet("provider-detail")]
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
            ProviderDetail.Command command)
        {
            command.ProviderId = providerInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(ProviderDetailConfirmation))
                        .WithProviderContext(providerInfo)
                        .WithMptxInstanceId(Flow)));
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
                            .WithMptxInstanceId(Flow)
                    }));
        }

        [HttpPost("hide-passed-notification")]
        public async Task<IActionResult> HidePassedNotication([LocalUrl] string returnUrl, HidePassedNotification.Command command)
        {
            command.ProviderId = ProviderContext.ProviderId;
            return await _mediator.SendAndMapResponse(command,
                success => Redirect(returnUrl));
        }
    }
}
