using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.Features.HelpdeskDashboard
{
    [Authorize(Policy = AuthorizationPolicyNames.Admin)]
    public class HelpdeskDashboardController : Controller
    {
        private readonly IMediator _mediator;

        public HelpdeskDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("HelpdeskDashboard")]
        public async Task<IActionResult> Index() =>
            await _mediator.SendAndMapResponse(new Query(), vm => View("Dashboard", vm));
    }
}
