using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Flurl;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ClassroomLocation = Dfc.CourseDirectory.WebV2.Features.Apprenticeships.ClassroomLocation;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    [Route("new-apprenticeship-provider")]
    [RequireProviderContext]
    [RestrictProviderTypes(ProviderType.Apprenticeships)]
    public class NewApprenticeshipProviderController : Controller, IMptxController<FlowModel>
    {
        private readonly IMediator _mediator;
        private readonly ProviderContext _providerContext;

        public NewApprenticeshipProviderController(IMediator mediator, IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _providerContext = providerContextProvider.GetProviderContext();
        }

        public MptxInstanceContext<FlowModel> Flow { get; set; }

        [MptxAction]
        [HttpGet("add-apprenticeship-classroom-location")]
        public IActionResult AddApprenticeshipClassroomLocation([FromServices] MptxManager mptxManager)
        {
            var childFlow = mptxManager.CreateInstance<ClassroomLocation.FlowModel, ClassroomLocation.IFlowModelCallback>(
                Flow.InstanceId,
                ClassroomLocation.FlowModel.Add(
                    _providerContext.ProviderInfo.ProviderId,
                    cancelable: (Flow.State.ApprenticeshipClassroomLocations?.Count ?? 0) > 0),
                contextItems: new Dictionary<string, object>()
                {
                    {
                        "ReturnUrl",
                        new Url(Url.Action(nameof(ApprenticeshipSummary)))
                            .WithMptxInstanceId(Flow.InstanceId)
                            .WithProviderContext(_providerContext)
                    }
                });
            return RedirectToAction("ClassroomLocation", "Apprenticeships")
                .WithMptxInstanceId(childFlow.InstanceId);
        }

        [MptxAction]
        [HttpGet("apprenticeship-details")]
        [AuthorizeApprenticeshipQASubmission]
        public async Task<IActionResult> ApprenticeshipDetails()
        {
            var query = new ApprenticeshipDetails.Query() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [MptxAction]
        [HttpPost("apprenticeship-details")]
        [AuthorizeApprenticeshipQASubmission]
        public async Task<IActionResult> ApprenticeshipDetails(ApprenticeshipDetails.Command command)
        {
            command.ProviderId = _providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => Flow.State.ApprenticeshipLocationType.HasValue ?
                        RedirectToAction(nameof(ApprenticeshipSummary))
                            .WithProviderContext(_providerContext)
                            .WithMptxInstanceId(Flow.InstanceId) :
                        RedirectToAction(nameof(ApprenticeshipLocations))
                            .WithProviderContext(_providerContext)
                            .WithMptxInstanceId(Flow.InstanceId)));
        }

        [MptxAction]
        [HttpGet("apprenticeship-employer-locations")]
        [AuthorizeApprenticeshipQASubmission]
        public async Task<IActionResult> ApprenticeshipEmployerLocations()
        {
            var query = new ApprenticeshipEmployerLocations.Query() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(
                query,
                response => View(response));
        }

        [MptxAction]
        [HttpPost("apprenticeship-employer-locations")]
        [AuthorizeApprenticeshipQASubmission]
        public async Task<IActionResult> ApprenticeshipEmployerLocations(ApprenticeshipEmployerLocations.Command command)
        {
            command.ProviderId = _providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success =>
                        (command.National.Value ?
                            Flow.State.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased ||
                                (Flow.State.ApprenticeshipClassroomLocations?.Count ?? 0) > 0 ?
                            RedirectToAction(nameof(ApprenticeshipSummary)) :
                            RedirectToAction(nameof(AddApprenticeshipClassroomLocation)) :
                            RedirectToAction(nameof(ApprenticeshipEmployerLocationsRegions)))
                        .WithProviderContext(_providerContext).WithMptxInstanceId(Flow.InstanceId)));
        }

        [MptxAction]
        [HttpGet("apprenticeship-employer-locations-regions")]
        [AuthorizeApprenticeshipQASubmission]
        public async Task<IActionResult> ApprenticeshipEmployerLocationsRegions()
        {
            var query = new ApprenticeshipEmployerLocationsRegions.Query() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, command => View(command));
        }

        [MptxAction]
        [HttpPost("apprenticeship-employer-locations-regions")]
        [AuthorizeApprenticeshipQASubmission]
        public async Task<IActionResult> ApprenticeshipEmployerLocationsRegions(
            ApprenticeshipEmployerLocationsRegions.Command command)
        {
            command.ProviderId = _providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success =>
                        (Flow.State.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased ||
                            (Flow.State.ApprenticeshipClassroomLocations?.Count ?? 0) > 0 ?
                            RedirectToAction(nameof(ApprenticeshipSummary)) :
                            RedirectToAction(nameof(AddApprenticeshipClassroomLocation)))
                                .WithProviderContext(_providerContext)
                                .WithMptxInstanceId(Flow)));
        }

        [MptxAction]
        [HttpGet("apprenticeship-locations")]
        [AuthorizeApprenticeshipQASubmission]
        public async Task<IActionResult> ApprenticeshipLocations()
        {
            var query = new ApprenticeshipLocations.Query() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(
                query,
                response => View(response));
        }

        [MptxAction]
        [HttpPost("apprenticeship-locations")]
        [AuthorizeApprenticeshipQASubmission]
        public async Task<IActionResult> ApprenticeshipLocations(ApprenticeshipLocations.Command command)
        {
            command.ProviderId = _providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success =>
                        (command.LocationType.Value switch
                        {
                            ApprenticeshipLocationType.ClassroomBased => RedirectToAction(nameof(AddApprenticeshipClassroomLocation)),
                            _ => RedirectToAction(nameof(ApprenticeshipEmployerLocations))
                        }).WithProviderContext(_providerContext).WithMptxInstanceId(Flow.InstanceId)));
        }
        [MptxAction]
        [HttpGet("apprenticeship-confirmation")]
        [AuthorizeApprenticeshipQASubmission]
        public async Task<IActionResult> ApprenticeshipSummary()
        {
            var query = new ApprenticeshipSummary.Query() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(
                query,
                response => response.Match(
                    errors => this.ViewFromErrors(errors),
                    vm => View(vm)));
        }

        [MptxAction]
        [HttpPost("apprenticeship-confirmation")]
        [AuthorizeApprenticeshipQASubmission]
        public async Task<IActionResult> ApprenticeshipSummaryConfirmation()
        {
            var command = new ApprenticeshipSummary.CompleteCommand() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match(
                    errors => this.ViewFromErrors("ApprenticeshipSummary", errors),
                    vm => View("Submitted")));
        }

        [MptxAction]
        [HttpGet("edit-apprenticeship-classroom-location")]
        public IActionResult EditApprenticeshipClassroomLocation(
            [FromQuery] Guid venueId,
            [FromServices] MptxManager mptxManager)
        {
            var location = Flow.State.ApprenticeshipClassroomLocations.GetValueOrDefault(venueId);

            if (location == null)
            {
                return new BadRequestResult();
            }

            var childFlow = mptxManager.CreateInstance<ClassroomLocation.FlowModel, ClassroomLocation.IFlowModelCallback>(
                Flow.InstanceId,
                ClassroomLocation.FlowModel.Edit(
                    _providerContext.ProviderInfo.ProviderId,
                    location.VenueId,
                    location.Radius,
                    location.DeliveryModes
                ),
                contextItems: new Dictionary<string, object>()
                {
                    {
                        "ReturnUrl",
                        new Url(Url.Action(nameof(ApprenticeshipSummary)))
                            .WithMptxInstanceId(Flow.InstanceId)
                            .WithProviderContext(_providerContext)
                    }
                });
            return RedirectToAction("ClassroomLocation", "Apprenticeships")
                .WithMptxInstanceId(childFlow.InstanceId);
        }

        [MptxAction]
        [HttpGet("find-standard")]
        public IActionResult FindStandard() => RedirectToAction(
            "Get",
            "FindStandard",
            new
            {
                returnUrl = new Url(Url.Action(nameof(StandardSelected)))
                    .WithMptxInstanceId(Flow.InstanceId)
                    .WithProviderContext(_providerContext)
            }).WithProviderContext(_providerContext);

        [MptxAction]
        [HttpGet("standard-selected")]
        public async Task<IActionResult> StandardSelected(Standard standard)
        {
            await _mediator.Send(new StandardSelected.Command() { Standard = standard });

            return RedirectToAction(nameof(ApprenticeshipDetails))
                .WithMptxInstanceId(Flow.InstanceId)
                .WithProviderContext(_providerContext);
        }

        [StartsMptx]
        [HttpGet("provider-detail")]
        [AuthorizeApprenticeshipQASubmission]
        public async Task<IActionResult> ProviderDetail(
            [FromServices] MptxManager mptxManager,
            [FromServices] FlowModelInitializer initializer)
        {
            var flowModel = await initializer.Initialize(_providerContext.ProviderInfo.ProviderId);
            var flow = mptxManager.CreateInstance(flowModel);
            return RedirectToAction(nameof(ProviderDetail))
                .WithMptxInstanceId(flow)
                .WithProviderContext(_providerContext);
        }

        [MptxAction]
        [HttpGet("provider-detail")]
        [AuthorizeApprenticeshipQASubmission]
        public async Task<IActionResult> ProviderDetail()
        {
            var query = new ProviderDetail.Query() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [MptxAction]
        [HttpPost("provider-detail")]
        [AuthorizeApprenticeshipQASubmission]
        public async Task<IActionResult> ProviderDetail(ProviderDetail.Command command)
        {
            command.ProviderId = _providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(ProviderDetailConfirmation))
                        .WithProviderContext(_providerContext)
                        .WithMptxInstanceId(Flow)));
        }

        [MptxAction]
        [HttpGet("provider-detail-confirmation")]
        [AuthorizeApprenticeshipQASubmission]
        public async Task<IActionResult> ProviderDetailConfirmation()
        {
            var query = new ProviderDetail.ConfirmationQuery() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [MptxAction]
        [HttpPost("provider-detail-confirmation")]
        [AuthorizeApprenticeshipQASubmission]
        public async Task<IActionResult> ProviderDetailConfirmation(ProviderDetail.ConfirmationCommand command)
        {
            command.ProviderId = _providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                success => Flow.State.ApprenticeshipId.HasValue ?
                    RedirectToAction(nameof(ApprenticeshipSummary))
                        .WithProviderContext(_providerContext)
                        .WithMptxInstanceId(Flow) :
                    RedirectToAction(nameof(FindStandard))
                        .WithProviderContext(_providerContext)
                        .WithMptxInstanceId(Flow));
        }

        [HttpPost("hide-passed-notification")]
        public async Task<IActionResult> HidePassedNotification(
            [LocalUrl] string returnUrl,
            HidePassedNotification.Command command)
        {
            command.ProviderId = _providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(command,
                success => Redirect(returnUrl));
        }
    }
}
