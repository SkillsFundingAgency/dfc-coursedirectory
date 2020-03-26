using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Providers
{
    [Route("providers")]
    [Authorize]
    public class ProvidersController : Controller
    {
        private readonly IMediator _mediator;

        public ProvidersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("info")]
        public async Task<IActionResult> EditProviderInfo(ProviderInfo providerInfo)
        {
            var query = new EditProviderInfo.Query() { ProviderId = providerInfo.ProviderId };
            return await _mediator.SendAndMapResponse(
                query,
                response => response.Match(
                    errors => this.ViewFromErrors(errors, statusCode: System.Net.HttpStatusCode.OK),
                    vm => View(vm)));
        }

        [HttpPost("info")]
        public async Task<IActionResult> EditProviderInfo(
            ProviderInfo providerInfo,
            EditProviderInfo.Command command)
        {
            command.ProviderId = providerInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction("Details", "Provider").WithProviderContext(providerInfo)));
        }
    }
}
