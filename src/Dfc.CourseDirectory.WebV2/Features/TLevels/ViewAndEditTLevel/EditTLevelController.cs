using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.ViewAndEditTLevel
{
    [Route("t-levels/{tLevelId}")]
    [JourneyMetadata(
        journeyName: "EditTLevel",
        stateType: typeof(EditTLevelJourneyModel),
        appendUniqueKey: true,
        requestDataKeys: "tLevelId")]
    [RestrictProviderTypes(ProviderType.TLevels)]
    [AuthorizeTLevel]
    public class EditTLevelController : Controller
    {
        private readonly IMediator _mediator;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;
        private readonly EditTLevelJourneyModelFactory _journeyModelFactory;
        private readonly IProviderContextProvider _providerContextProvider;
        private JourneyInstance<EditTLevelJourneyModel> _journeyInstance;

        public EditTLevelController(
            IMediator mediator,
            JourneyInstanceProvider journeyInstanceProvider,
            EditTLevelJourneyModelFactory journeyModelFactory,
            IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _journeyInstanceProvider = journeyInstanceProvider;
            _journeyModelFactory = journeyModelFactory;
            _providerContextProvider = providerContextProvider;
        }

        [HttpGet("edit")]
        public async Task<IActionResult> Edit(
            [FromQuery] Guid? venueId)  // Populated by the Add Venue journey
        {
            var query = new EditTLevel.Query();
            return await _mediator.SendAndMapResponse(
                query,
                vm =>
                {
                    // If we've just added a new venue, ensure it's selected
                    if (venueId.HasValue && vm.ProviderVenues.Any(v => v.VenueId == venueId))
                    {
                        vm.LocationVenueIds.Add(venueId.Value);
                    }

                    return View("EditTLevel", vm);
                });
        }

        [HttpPost("edit")]
        public async Task<IActionResult> Edit(Guid tLevelId, EditTLevel.Command command) =>
            await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors("EditTLevel", errors),
                    success => RedirectToAction(nameof(CheckAndPublish), new { tLevelId })
                        .WithJourneyInstanceUniqueKey(_journeyInstance)));

        [HttpGet("cancel")]
        public IActionResult Cancel(Guid tLevelId)
        {
            _journeyInstance.Delete();

            return RedirectToAction(nameof(ViewTLevelController.Index), "ViewTLevel", new { tLevelId });
        }

        [HttpPost("add-location")]
        public async Task<IActionResult> AddAnotherLocation(
            Guid tLevelId,
            Save.Command command)
        {
            await _mediator.Send(command);

            return RedirectToAction(
                "Index",
                "AddVenue",
                new
                {
                    returnUrl = Url.Action(
                        nameof(Edit),
                        new { tLevelId, ffiid = _journeyInstance.InstanceId.UniqueKey })
                })
                .WithProviderContext(_providerContextProvider.GetProviderContext());
        }

        [HttpGet("check-publish")]
        public async Task<IActionResult> CheckAndPublish()
        {
            var query = new CheckAndPublish.Query();
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [HttpPost("check-publish")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Publish(Guid tLevelId)
        {
            var command = new CheckAndPublish.Command();
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(nameof(CheckAndPublish), errors),
                    success => RedirectToAction(nameof(Published), new { tLevelId })
                        .WithJourneyInstanceUniqueKey(_journeyInstance)));
        }

        [HttpGet("success")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Published()
        {
            var query = new Published.Query();

            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            string commonurl = "find-a-course/tdetails?tlevelId=";
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) + commonurl;

            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // We need to be in a FormFlow journey for all of these pages (so we have somewhere to stage the changes)

            var tLevelId = Guid.Parse((string)context.RouteData.Values["tLevelId"]);

            _journeyInstance = await _journeyInstanceProvider.GetOrCreateInstanceAsync(() =>
                _journeyModelFactory.CreateModel(tLevelId));

            if (!_journeyInstanceProvider.IsCurrentInstance(_journeyInstance))
            {
                context.Result = RedirectToActionPreserveMethod(
                        ControllerContext.ActionDescriptor.ActionName,
                        routeValues: new { tLevelId })
                    .WithJourneyInstanceUniqueKey(_journeyInstance);

                return;
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
