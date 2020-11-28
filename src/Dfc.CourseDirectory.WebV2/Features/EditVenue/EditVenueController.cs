﻿using System;
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
        public IActionResult Cancel(FormFlowInstance formFlowInstance)
        {
            formFlowInstance.Delete();

            return RedirectToAction("Index", "Venues");
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
