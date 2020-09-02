using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Providers
{
    [Route("providers")]
    public class ProvidersController : Controller
    {
        private readonly IMediator _mediator;

        public ProvidersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("info")]
        public async Task<IActionResult> EditProviderInfo(ProviderContext providerContext)
        {
            var query = new EditProviderInfo.Query() { ProviderId = providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(
                query,
                response => response.Match(
                    errors => this.ViewFromErrors(errors, statusCode: System.Net.HttpStatusCode.OK),
                    vm => View(vm)));
        }

        [HttpPost("info")]
        public async Task<IActionResult> EditProviderInfo(
            EditProviderInfo.Command command,
            ProviderContext providerContext)
        {
            command.ProviderId = providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction("Details", "Provider").WithProviderContext(providerContext)));
        }

        [HttpGet("")]
        [AssignLegacyProviderContext]  // This can go once the 'edit provider type' journey is in v2
        public async Task<IActionResult> ProviderDetails(ProviderContext providerContext)
        {
            var request = new ProviderDetails.Query() { ProviderId = providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(request, vm => View(vm));
        }
    }
}
