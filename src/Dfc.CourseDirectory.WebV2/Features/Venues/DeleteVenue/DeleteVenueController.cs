using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.DeleteVenue
{
    [JourneyMetadata(
        journeyName: nameof(DeleteVenue),
        stateType: typeof(JourneyModel),
        appendUniqueKey: false,
        requestDataKeys: new[] { nameof(Command.VenueId) })]
    [RequireProviderContext]
    [Route("venues/{venueId}")]
    public class DeleteVenueController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IProviderContextProvider _providerContextProvider;

        public DeleteVenueController(IMediator mediator, IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _providerContextProvider = providerContextProvider;
        }

        [AuthorizeVenue]
        [HttpGet("delete")]
        public Task<IActionResult> DeleteVenue(Guid venueId) =>
            _mediator.SendAndMapResponse(
                new Query
                {
                    VenueId = venueId,
                    ProviderId = _providerContextProvider.GetProviderContext().ProviderInfo.ProviderId
                },
                r => r.Match<IActionResult>(
                    _ => NotFound(),
                    vm => View(vm)));

        [AuthorizeVenue]
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteVenue(Command request)
        {
            var providerContext = _providerContextProvider.GetProviderContext();

            if (request.ProviderId != providerContext.ProviderInfo.ProviderId)
            {
                return BadRequest();
            }

            return await _mediator.SendAndMapResponse(
                request,
                r => r.Match<IActionResult>(
                    _ => NotFound(),
                    errors => this.ViewFromErrors(errors),
                    _ => RedirectToAction(nameof(VenueDeleted), new { request.VenueId })
                        .WithProviderContext(providerContext)));
        }

        [RequireJourneyInstance]
        [HttpGet("delete-success")]
        public Task<IActionResult> VenueDeleted() =>
            _mediator.SendAndMapResponse<DeletedViewModel, IActionResult>(new DeletedQuery(), vm => View(vm));
    }
}
