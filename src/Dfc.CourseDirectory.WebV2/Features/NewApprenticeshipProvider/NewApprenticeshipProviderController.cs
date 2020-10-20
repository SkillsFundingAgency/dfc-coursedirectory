using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Flurl;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ClassroomLocation = Dfc.CourseDirectory.WebV2.Features.Apprenticeships.ClassroomLocation;
using FindStandard = Dfc.CourseDirectory.WebV2.Features.Apprenticeships.FindStandard;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    [Route("new-apprenticeship-provider")]
    [RequiresProviderContext]
    public class NewApprenticeshipProviderController : Controller, IMptxController<FlowModel>
    {
        private readonly IMediator _mediator;

        public NewApprenticeshipProviderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public MptxInstanceContext<FlowModel> Flow { get; set; }

        [MptxAction]
        [HttpGet("add-apprenticeship-classroom-location")]
        public IActionResult AddApprenticeshipClassroomLocation(
            [FromServices] MptxManager mptxManager,
            ProviderContext providerContext)
        {
            var childFlow = mptxManager.CreateInstance<ClassroomLocation.FlowModel, ClassroomLocation.IFlowModelCallback>(
                Flow.InstanceId,
                ClassroomLocation.FlowModel.Add(
                    providerContext.ProviderInfo.ProviderId,
                    cancelable: (Flow.State.ApprenticeshipClassroomLocations?.Count ?? 0) > 0),
                contextItems: new Dictionary<string, object>()
                {
                    {
                        "ReturnUrl",
                        new Url(Url.Action(nameof(ApprenticeshipSummary)))
                            .WithMptxInstanceId(Flow.InstanceId)
                            .WithProviderContext(providerContext)
                    }
                });
            return RedirectToAction("ClassroomLocation", "Apprenticeships")
                .WithMptxInstanceId(childFlow.InstanceId);
        }

        [MptxAction]
        [HttpGet("apprenticeship-details")]
        public async Task<IActionResult> ApprenticeshipDetails(ProviderContext providerContext)
        {
            var query = new ApprenticeshipDetails.Query() { ProviderId = providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [MptxAction]
        [HttpPost("apprenticeship-details")]
        public async Task<IActionResult> ApprenticeshipDetails(
            ApprenticeshipDetails.Command command,
            ProviderContext providerContext)
        {
            command.ProviderId = providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => Flow.State.ApprenticeshipLocationType.HasValue ?
                        RedirectToAction(nameof(ApprenticeshipSummary))
                            .WithProviderContext(providerContext)
                            .WithMptxInstanceId(Flow.InstanceId) :
                        RedirectToAction(nameof(ApprenticeshipLocations))
                            .WithProviderContext(providerContext)
                            .WithMptxInstanceId(Flow.InstanceId)));
        }

        [MptxAction]
        [HttpGet("apprenticeship-employer-locations")]
        public async Task<IActionResult> ApprenticeshipEmployerLocations(ProviderContext providerContext)
        {
            var query = new ApprenticeshipEmployerLocations.Query() { ProviderId = providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(
                query,
                response => View(response));
        }

        [MptxAction]
        [HttpPost("apprenticeship-employer-locations")]
        public async Task<IActionResult> ApprenticeshipEmployerLocations(
            ApprenticeshipEmployerLocations.Command command,
            ProviderContext providerContext)
        {
            command.ProviderId = providerContext.ProviderInfo.ProviderId;
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
                        .WithProviderContext(providerContext).WithMptxInstanceId(Flow.InstanceId)));
        }

        [MptxAction]
        [HttpGet("apprenticeship-employer-locations-regions")]
        public async Task<IActionResult> ApprenticeshipEmployerLocationsRegions(ProviderContext providerContext)
        {
            var query = new ApprenticeshipEmployerLocationsRegions.Query() { ProviderId = providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, command => View(command));
        }

        [MptxAction]
        [HttpPost("apprenticeship-employer-locations-regions")]
        public async Task<IActionResult> ApprenticeshipEmployerLocationsRegions(
            ApprenticeshipEmployerLocationsRegions.Command command,
            ProviderContext providerContext)
        {
            command.ProviderId = providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success =>
                        (Flow.State.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased ||
                            (Flow.State.ApprenticeshipClassroomLocations?.Count ?? 0) > 0 ?
                            RedirectToAction(nameof(ApprenticeshipSummary)) :
                            RedirectToAction(nameof(AddApprenticeshipClassroomLocation)))
                                .WithProviderContext(providerContext)
                                .WithMptxInstanceId(Flow)));
        }

        [MptxAction]
        [HttpGet("apprenticeship-locations")]
        public async Task<IActionResult> ApprenticeshipLocations(ProviderContext providerContext)
        {
            var query = new ApprenticeshipLocations.Query() { ProviderId = providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(
                query,
                response => View(response));
        }

        [MptxAction]
        [HttpPost("apprenticeship-locations")]
        public async Task<IActionResult> ApprenticeshipLocations(
            ApprenticeshipLocations.Command command,
            ProviderContext providerContext)
        {
            command.ProviderId = providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success =>
                        (command.LocationType.Value switch
                        {
                            ApprenticeshipLocationType.ClassroomBased => RedirectToAction(nameof(AddApprenticeshipClassroomLocation)),
                            _ => RedirectToAction(nameof(ApprenticeshipEmployerLocations))
                        }).WithProviderContext(providerContext).WithMptxInstanceId(Flow.InstanceId)));
        }
        [MptxAction]
        [HttpGet("apprenticeship-confirmation")]
        public async Task<IActionResult> ApprenticeshipSummary(ProviderContext providerContext)
        {
            var query = new ApprenticeshipSummary.Query() { ProviderId = providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(
                query,
                response => response.Match(
                    errors => this.ViewFromErrors(errors),
                    vm => View(vm)));
        }

        [MptxAction]
        [HttpPost("apprenticeship-confirmation")]
        public async Task<IActionResult> ApprenticeshipSummaryConfirmation(ProviderContext providerContext)
        {
            var command = new ApprenticeshipSummary.CompleteCommand() { ProviderId = providerContext.ProviderInfo.ProviderId };
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
            [FromServices] MptxManager mptxManager,
            ProviderContext providerContext)
        {
            var location = Flow.State.ApprenticeshipClassroomLocations.GetValueOrDefault(venueId);

            if (location == null)
            {
                return new BadRequestResult();
            }

            var childFlow = mptxManager.CreateInstance<ClassroomLocation.FlowModel, ClassroomLocation.IFlowModelCallback>(
                Flow.InstanceId,
                ClassroomLocation.FlowModel.Edit(
                    providerContext.ProviderInfo.ProviderId,
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
                            .WithProviderContext(providerContext)
                    }
                });
            return RedirectToAction("ClassroomLocation", "Apprenticeships")
                .WithMptxInstanceId(childFlow.InstanceId);
        }

        [MptxAction]
        [HttpGet("find-standard")]
        public IActionResult FindStandard(ProviderContext providerContext) => RedirectToAction(
            "Get",
            "FindStandard",
            new
            {
                returnUrl = new Url(Url.Action(nameof(StandardSelected)))
                    .WithMptxInstanceId(Flow.InstanceId)
                    .WithProviderContext(providerContext)
            }).WithProviderContext(providerContext);

        [MptxAction]
        [HttpGet("standard-selected")]
        public async Task<IActionResult> StandardSelected(Standard standard, ProviderContext providerContext)
        {
            await _mediator.Send(new StandardSelected.Command() { Standard = standard });

            return RedirectToAction(nameof(ApprenticeshipDetails))
                .WithMptxInstanceId(Flow.InstanceId)
                .WithProviderContext(providerContext);
        }

        [StartsMptx]
        [HttpGet("provider-detail")]
        public async Task<IActionResult> ProviderDetail(
            [FromServices] MptxManager mptxManager,
            [FromServices] FlowModelInitializer initializer,
            ProviderContext providerContext)
        {
            var flowModel = await initializer.Initialize(providerContext.ProviderInfo.ProviderId);
            var flow = mptxManager.CreateInstance(flowModel);
            return RedirectToAction(nameof(ProviderDetail))
                .WithMptxInstanceId(flow)
                .WithProviderContext(providerContext);
        }

        [MptxAction]
        [HttpGet("provider-detail")]
        public async Task<IActionResult> ProviderDetail(ProviderContext providerContext)
        {
            var query = new ProviderDetail.Query() { ProviderId = providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [MptxAction]
        [HttpPost("provider-detail")]
        public async Task<IActionResult> ProviderDetail(
            ProviderDetail.Command command,
            ProviderContext providerContext)
        {
            command.ProviderId = providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(ProviderDetailConfirmation))
                        .WithProviderContext(providerContext)
                        .WithMptxInstanceId(Flow)));
        }

        [MptxAction]
        [HttpGet("provider-detail-confirmation")]
        public async Task<IActionResult> ProviderDetailConfirmation(ProviderContext providerContext)
        {
            var query = new ProviderDetail.ConfirmationQuery() { ProviderId = providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [MptxAction]
        [HttpPost("provider-detail-confirmation")]
        public async Task<IActionResult> ProviderDetailConfirmation(
            ProviderContext providerContext,
            ProviderDetail.ConfirmationCommand command)
        {
            command.ProviderId = providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                success => Flow.State.ApprenticeshipId.HasValue ?
                    RedirectToAction(nameof(ApprenticeshipSummary))
                        .WithProviderContext(providerContext)
                        .WithMptxInstanceId(Flow) :
                    RedirectToAction(nameof(FindStandard))
                        .WithProviderContext(providerContext)
                        .WithMptxInstanceId(Flow));
        }

        [HttpPost("hide-passed-notification")]
        public async Task<IActionResult> HidePassedNotification(
            [LocalUrl] string returnUrl,
            HidePassedNotification.Command command,
            ProviderContext providerContext)
        {
            command.ProviderId = providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(command,
                success => Redirect(returnUrl));
        }
    }
}
