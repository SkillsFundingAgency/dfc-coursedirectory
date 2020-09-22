using System;
using System.Threading.Tasks;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.WebV2.Features.EditVenue
{
    [Route("venues/{venueId:guid}")]
    [FormFlowAction(key: "EditVenue", stateType: typeof(EditVenueFlowModel), idRouteParameterNames: "venueId")]
    public class EditVenueController : Controller
    {
        private readonly IMediator _mediator;
        private readonly FormFlowInstanceFactory _formFlowInstanceFactory;
        private readonly EditVenueFlowModelFactory _editVenueFlowModelFactory;

        public EditVenueController(
            IMediator mediator,
            FormFlowInstanceFactory formFlowInstanceFactory,
            EditVenueFlowModelFactory editVenueFlowModelFactory)
        {
            _mediator = mediator;
            _formFlowInstanceFactory = formFlowInstanceFactory;
            _editVenueFlowModelFactory = editVenueFlowModelFactory;
        }

        [HttpGet("")]
        public IActionResult Details(Guid venueId) => RedirectToAction("EditVenue", "Venues", new { Id = venueId });

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

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await _formFlowInstanceFactory.GetOrCreateInstanceAsync(() =>
            {
                var venueId = Guid.Parse((string)context.RouteData.Values["venueId"]);
                return _editVenueFlowModelFactory.CreateModel(venueId);
            });

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
