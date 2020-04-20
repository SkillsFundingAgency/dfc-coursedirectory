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
using FindStandardOrFramework = Dfc.CourseDirectory.WebV2.Features.Apprenticeships.FindStandardOrFramework;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    [Route("new-apprenticeship-provider")]
    [RequireFeatureFlag(FeatureFlags.ApprenticeshipQA)]
    public class NewApprenticeshipProviderController :
        Controller,
        IMptxController<FlowModel>,
        IRequiresProviderContextController
    {
        private readonly IMediator _mediator;

        public NewApprenticeshipProviderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public MptxInstanceContext<FlowModel> Flow { get; set; }

        public ProviderInfo ProviderContext { get; set; }

        [MptxAction]
        [HttpGet("add-apprenticeship-classroom-location")]
        public IActionResult AddApprenticeshipClassroomLocation([FromServices] MptxManager mptxManager)
        {
            var childFlow = mptxManager.CreateInstance<ClassroomLocation.FlowModel, ClassroomLocation.IFlowModelCallback>(
                Flow.InstanceId,
                ClassroomLocation.FlowModel.Add(ProviderContext.ProviderId),
                contextItems: new Dictionary<string, object>()
                {
                    {
                        "ReturnUrl",
                        new Url(Url.Action(nameof(ApprenticeshipSummary)))
                            .WithMptxInstanceId(Flow.InstanceId)
                            .WithProviderContext(ProviderContext)
                    }
                });
            return RedirectToAction("ClassroomLocation", "Apprenticeships")
                .WithMptxInstanceId(childFlow.InstanceId);
        }

        [MptxAction]
        [HttpGet("apprenticeship-details")]
        public async Task<IActionResult> ApprenticeshipDetails()
        {
            var query = new ApprenticeshipDetails.Query() { ProviderId = ProviderContext.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [MptxAction]
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

        [MptxAction]
        [HttpGet("apprenticeship-employer-locations")]
        public async Task<IActionResult> ApprenticeshipEmployerLocations()
        {
            var query = new ApprenticeshipEmployerLocations.Query() { ProviderId = ProviderContext.ProviderId };
            return await _mediator.SendAndMapResponse(
                query,
                response => View(response));
        }

        [MptxAction]
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
                            Flow.State.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased ||
                                (Flow.State.ApprenticeshipClassroomLocations?.Count ?? 0) > 0 ?
                            RedirectToAction(nameof(ApprenticeshipSummary)) :
                            RedirectToAction(nameof(AddApprenticeshipClassroomLocation)) :
                            RedirectToAction(nameof(ApprenticeshipEmployerLocationsRegions)))
                        .WithProviderContext(ProviderContext).WithMptxInstanceId(Flow.InstanceId)));
        }

        [MptxAction]
        [HttpGet("apprenticeship-employer-locations-regions")]
        public async Task<IActionResult> ApprenticeshipEmployerLocationsRegions()
        {
            var query = new ApprenticeshipEmployerLocationsRegions.Query() { ProviderId = ProviderContext.ProviderId };
            return await _mediator.SendAndMapResponse(query, command => View(command));
        }

        [MptxAction]
        [HttpPost("apprenticeship-employer-locations-regions")]
        public async Task<IActionResult> ApprenticeshipEmployerLocationsRegions(ApprenticeshipEmployerLocationsRegions.Command command)
        {
            command.ProviderId = ProviderContext.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success =>
                        (Flow.State.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased ||
                            (Flow.State.ApprenticeshipClassroomLocations?.Count ?? 0) > 0 ?
                            RedirectToAction(nameof(ApprenticeshipSummary)) :
                            RedirectToAction(nameof(AddApprenticeshipClassroomLocation)))
                                .WithProviderContext(ProviderContext)
                                .WithMptxInstanceId(Flow)));
        }

        [MptxAction]
        [HttpGet("apprenticeship-locations")]
        public async Task<IActionResult> ApprenticeshipLocations()
        {
            var query = new ApprenticeshipLocations.Query() { ProviderId = ProviderContext.ProviderId };
            return await _mediator.SendAndMapResponse(
                query,
                response => View(response));
        }

        [MptxAction]
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
                            ApprenticeshipLocationType.ClassroomBased => RedirectToAction(nameof(AddApprenticeshipClassroomLocation)),
                            _ => RedirectToAction(nameof(ApprenticeshipEmployerLocations))
                        }).WithProviderContext(ProviderContext).WithMptxInstanceId(Flow.InstanceId)));
        }
        [MptxAction]
        [HttpGet("apprenticeship-confirmation")]
        public async Task<IActionResult> ApprenticeshipSummary()
        {
            var query = new ApprenticeshipSummary.Query() { ProviderId = ProviderContext.ProviderId };
            return await _mediator.SendAndMapResponse(
                query,
                response => response.Match(
                    errors => this.ViewFromErrors(errors),
                    vm => View(vm)));
        }

        [MptxAction]
        [HttpPost("apprenticeship-confirmation")]
        public async Task<IActionResult> ApprenticeshipSummaryConfirmation()
        {
            var command = new ApprenticeshipSummary.CompleteCommand() { ProviderId = ProviderContext.ProviderId };
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
                    ProviderContext.ProviderId,
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
                            .WithProviderContext(ProviderContext)
                    }
                });
            return RedirectToAction("ClassroomLocation", "Apprenticeships")
                .WithMptxInstanceId(childFlow.InstanceId);
        }

        [MptxAction]
        [HttpGet("find-standard")]
        public IActionResult FindStandardOrFramework([FromServices] MptxManager mptxManager)
        {
            var childFlow = mptxManager.CreateInstance<FindStandardOrFramework.FlowModel, FindStandardOrFramework.IFlowModelCallback>(
                Flow.InstanceId,
                new FindStandardOrFramework.FlowModel()
                {
                    ProviderId = ProviderContext.ProviderId
                },
                contextItems: new Dictionary<string, object>()
                {
                    {
                        "ReturnUrl",
                        new Url(Url.Action(nameof(ApprenticeshipDetails)))
                            .WithMptxInstanceId(Flow.InstanceId)
                            .WithProviderContext(ProviderContext)
                    }
                });
            return RedirectToAction("FindStandardOrFramework", "Apprenticeships")
                .WithMptxInstanceId(childFlow.InstanceId);
        }

        [StartsMptx]
        [HttpGet("provider-detail")]
        public async Task<IActionResult> ProviderDetail(
            [FromServices] MptxManager mptxManager,
            [FromServices] FlowModelInitializer initializer)
        {
            var flowModel = await initializer.Initialize(ProviderContext.ProviderId);
            var flow = mptxManager.CreateInstance(flowModel);
            return RedirectToAction(nameof(ProviderDetail))
                .WithMptxInstanceId(flow);
        }

        [MptxAction]
        [HttpGet("provider-detail")]
        public async Task<IActionResult> ProviderDetail()
        {
            var query = new ProviderDetail.Query() { ProviderId = ProviderContext.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [MptxAction]
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

        [MptxAction]
        [HttpGet("provider-detail-confirmation")]
        public async Task<IActionResult> ProviderDetailConfirmation()
        {
            var query = new ProviderDetail.ConfirmationQuery() { ProviderId = ProviderContext.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [MptxAction]
        [HttpPost("provider-detail-confirmation")]
        public async Task<IActionResult> ProviderDetailConfirmation(
            ProviderInfo providerInfo,
            ProviderDetail.ConfirmationCommand command)
        {
            command.ProviderId = providerInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                success => RedirectToAction(nameof(FindStandardOrFramework))
                    .WithMptxInstanceId(Flow));
        }

        [HttpPost("hide-passed-notification")]
        public async Task<IActionResult> HidePassedNotification(
            [LocalUrl] string returnUrl,
            HidePassedNotification.Command command)
        {
            command.ProviderId = ProviderContext.ProviderId;
            return await _mediator.SendAndMapResponse(command,
                success => Redirect(returnUrl));
        }
    }
}
