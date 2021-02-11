using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.ViewVenues
{
    [RequireProviderContext]
    [Route("venues")]
    public class ViewVenuesController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IProviderContextProvider _providerContextProvider;

        public ViewVenuesController(IMediator mediator, IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _providerContextProvider = providerContextProvider;
        }

        [HttpGet]
        public Task<IActionResult> ViewVenues() =>
            _mediator.SendAndMapResponse<ViewModel, IActionResult>(
                new Query { ProviderId = _providerContextProvider.GetProviderContext().ProviderInfo.ProviderId },
                vm => View(vm));
    }
}
