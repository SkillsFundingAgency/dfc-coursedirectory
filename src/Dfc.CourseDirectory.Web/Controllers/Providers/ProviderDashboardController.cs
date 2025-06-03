using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Attributes;
using Dfc.CourseDirectory.Core.Extensions;
using Dfc.CourseDirectory.Core.Middleware;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers.Providers
{
    [Route("/")]
    public class ProviderDashboardController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ProviderContext _providerContext;

        public ProviderDashboardController(IMediator mediator, IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _providerContext = providerContextProvider.GetProviderContext();
        }

        [RequireProviderContext]
        [HttpGet("dashboard")]
        public async Task<IActionResult> Index()
        {
            var query = new ViewModels.Providers.Dashboard.Query() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View("Dashboard", vm));
        }
    }
}
