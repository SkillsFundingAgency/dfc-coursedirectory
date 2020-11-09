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

        [RequireFeatureFlag(FeatureFlags.ProviderDisplayName)]
        [HttpGet("display-name")]
        public async Task<IActionResult> DisplayName(ProviderContext providerContext)
        {
            var query = new DisplayName.Query() { ProviderId = providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [RequireFeatureFlag(FeatureFlags.ProviderDisplayName)]
        [HttpPost("display-name")]
        public async Task<IActionResult> DisplayName(DisplayName.Command command, ProviderContext providerContext)
        {
            command.ProviderId = providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                success => RedirectToAction(nameof(ProviderDetails)).WithProviderContext(providerContext));
        }

        [HttpGet("info")]
        [AuthorizeAdmin]
        public async Task<IActionResult> EditProviderInfo(ProviderContext providerContext)
        {
            var query = new EditProviderInfo.Query() { ProviderId = providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(
                query,
                command => View(command));
        }

        [HttpPost("info")]
        [AuthorizeAdmin]
        public async Task<IActionResult> EditProviderInfo(
            EditProviderInfo.Command command,
            ProviderContext providerContext)
        {
            command.ProviderId = providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(ProviderDetails)).WithProviderContext(providerContext)));
        }

        [HttpGet("")]
        public async Task<IActionResult> ProviderDetails(ProviderContext providerContext)
        {
            var request = new ProviderDetails.Query() { ProviderId = providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(request, vm => View(vm));
        }
    }
}
