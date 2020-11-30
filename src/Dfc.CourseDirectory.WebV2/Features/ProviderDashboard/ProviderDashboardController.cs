using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.ProviderDashboard
{
    public class ProviderDashboardController : Controller
    {
        private readonly IMediator _mediator;

        public ProviderDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("dashboard-beta")]
        public async Task<IActionResult> Index(ProviderContext providerContext)
        {
            var query = new Dashboard.Query() { ProviderId = providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View("Dashboard", vm));
        }
    }
}
