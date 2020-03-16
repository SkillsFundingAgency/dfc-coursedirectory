using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Features.HelpdeskDashboard.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.HelpdeskDashboard
{
    public class HelpdeskDashboardController : Controller
    {
        [HttpGet("helpdesk-dashboard")]
        public async Task<IActionResult> Dashboard([FromServices] IMediator mediator)
        {
            var result = await mediator.Send(new Query());
            return result.Match(
                error => error.Value == ErrorReason.NotAuthorized ?
                    Forbid() : error.Value == ErrorReason.NothingAvailable ?
                    (IActionResult)NotFound() : BadRequest(),
                vm => View(vm));
        }
    }
}
