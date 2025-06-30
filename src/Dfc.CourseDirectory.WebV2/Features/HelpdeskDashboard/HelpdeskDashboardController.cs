using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dfc.CourseDirectory.Core.Extensions;
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

        [HttpGet("helpdesk-dashboard")]
        public async Task<IActionResult> Dashboard() =>
            await _mediator.SendAndMapResponse(new Query(), vm => View("Dashboard", vm));
    }
}
