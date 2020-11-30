using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.WebV2.Features.EditVenue
{
    [Route("venues/{venueId:guid}")]
    [AuthorizeVenue(venueIdRouteParameterName: "venueId")]
    [JourneyMetadata(
        journeyName: "EditVenue",
        stateType: typeof(EditVenueJourneyModel),
        appendUniqueKey: false,
        requestDataKeys: "venueId")]
    public class EditVenueController : Controller
    {
        private readonly IMediator _mediator;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;
        private JourneyInstance<EditVenueJourneyModel> _journeyInstance;
        private readonly EditVenueJourneyModelFactory _editVenueJourneyModelFactory;

        public EditVenueController(
            IMediator mediator,
            JourneyInstanceProvider journeyInstanceProvider,
            EditVenueJourneyModelFactory editVenueJourneyModelFactory)
        {
            _mediator = mediator;
            _journeyInstanceProvider = journeyInstanceProvider;
            _editVenueJourneyModelFactory = editVenueJourneyModelFactory;
        }

        [HttpGet("")]
        public async Task<IActionResult> Details(Details.Query query) =>
            await _mediator.SendAndMapResponse(query, vm => View(vm));

        [HttpGet("address")]
        public async Task<IActionResult> Address(Address.Query request) =>
            await _mediator.SendAndMapResponse(request, vm => View(vm));

        [HttpPost("address")]
        public async Task<IActionResult> Address(Guid venueId, Address.Command request)
        {
            request.VenueId = venueId;
            return await _mediator.SendAndMapResponse(
                request,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(Details), new { venueId })));
        }

        [HttpGet("email")]
        public async Task<IActionResult> Email(Email.Query request) =>
            await _mediator.SendAndMapResponse(request, vm => View(vm));

        [HttpPost("email")]
        public async Task<IActionResult> Email(Guid venueId, Email.Command request)
        {
            request.VenueId = venueId;
            return await _mediator.SendAndMapResponse(
                request,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(Details), new { venueId })));
        }

        [HttpGet("name")]
        public async Task<IActionResult> Name(Name.Query request) =>
            await _mediator.SendAndMapResponse(request, vm => View(vm));

        [HttpPost("name")]
        public async Task<IActionResult> Name(Guid venueId, Name.Command request)
        {
            request.VenueId = venueId;
            return await _mediator.SendAndMapResponse(
                request,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(Details), new { venueId })));
        }

        [HttpGet("phone-number")]
        public async Task<IActionResult> PhoneNumber(PhoneNumber.Query request) =>
            await _mediator.SendAndMapResponse(request, vm => View(vm));

        [HttpPost("phone-number")]
        public async Task<IActionResult> PhoneNumber(Guid venueId, PhoneNumber.Command request)
        {
            request.VenueId = venueId;
            return await _mediator.SendAndMapResponse(
                request,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(Details), new { venueId })));
        }

        [HttpGet("website")]
        public async Task<IActionResult> Website(Website.Query request) =>
            await _mediator.SendAndMapResponse(request, vm => View(vm));

        [HttpPost("website")]
        public async Task<IActionResult> Website(Guid venueId, Website.Command request)
        {
            request.VenueId = venueId;
            return await _mediator.SendAndMapResponse(
                request,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(Details), new { venueId })));
        }

        [HttpPost("")]
        public async Task<IActionResult> Save(Guid venueId, Save.Command command)
        {
            command.VenueId = venueId;
            return await _mediator.SendAndMapResponse(
                command,
                success =>
                {
                    TempData[TempDataKeys.UpdatedVenueId] = venueId;

                    return RedirectToAction("Index", "Venues");
                });
        }

        [HttpPost("cancel")]
        public IActionResult Cancel()
        {
            _journeyInstance.Delete();

            return RedirectToAction("Index", "Venues");
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _journeyInstance = await _journeyInstanceProvider.GetOrCreateInstanceAsync(() =>
            {
                var venueId = Guid.Parse((string)context.RouteData.Values["venueId"]);
                return _editVenueJourneyModelFactory.CreateModel(venueId);
            });

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
