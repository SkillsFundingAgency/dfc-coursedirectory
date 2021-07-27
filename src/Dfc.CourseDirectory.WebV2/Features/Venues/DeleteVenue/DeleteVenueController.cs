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
        public async Task<IActionResult> DeleteVenue(Guid venueId) =>
            await _mediator.SendAndMapResponse(
                new Query
                {
                    VenueId = venueId,
                    ProviderId = _providerContextProvider.GetProviderContext().ProviderInfo.ProviderId
                },
                vm => View(vm));

        [AuthorizeVenue]
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteVenue(Command request) => await _mediator.SendAndMapResponse(
            request,
            r => r.Match<IActionResult>(
                errors => this.ViewFromErrors(errors),
                _ => RedirectToAction(nameof(VenueDeleted), new { request.VenueId })
                    .WithProviderContext(_providerContextProvider.GetProviderContext())));

        [RequireJourneyInstance]
        [HttpGet("delete-success")]
        public Task<IActionResult> VenueDeleted() =>
            _mediator.SendAndMapResponse<DeletedViewModel, IActionResult>(new DeletedQuery(), vm => View(vm));
    }
}
