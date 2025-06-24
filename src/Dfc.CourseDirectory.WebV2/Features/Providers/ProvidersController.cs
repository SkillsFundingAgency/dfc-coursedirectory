using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Attributes;
using Dfc.CourseDirectory.Core.Extensions;
using Dfc.CourseDirectory.Core.Middleware;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Providers
{
    [Route("providers")]
    [RequireProviderContext]
    public class ProvidersController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ProviderContext _providerContext;

        public ProvidersController(IMediator mediator, IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _providerContext = providerContextProvider.GetProviderContext();
        }

        [HttpGet("display-name")]
        public async Task<IActionResult> DisplayName()
        {
            var query = new WebV2.Features.Providers.DisplayName.Query() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [HttpPost("display-name")]
        public async Task<IActionResult> DisplayName(WebV2.Features.Providers.DisplayName.Command command)
        {
            command.ProviderId = _providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                success => RedirectToAction(nameof(ProviderDetails)).WithProviderContext(_providerContext));
        }

        [HttpGet("")]
        public async Task<IActionResult> ProviderDetails()
        {
            var request = new WebV2.Features.Providers.ProviderDetails.Query() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(request, vm => View(vm));
        }
        [HttpGet("info")]
        [AuthorizeAdmin]
        public async Task<IActionResult> EditProviderInfo()
        {
            var query = new WebV2.Features.Providers.EditProviderInfo.Query() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(
                query,
                command => View(command));
        }

        [HttpPost("info")]
        [AuthorizeAdmin]
        public async Task<IActionResult> EditProviderInfo(WebV2.Features.Providers.EditProviderInfo.Command command)
        {
            command.ProviderId = _providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(ProviderDetails)).WithProviderContext(_providerContext)));
        }
    }
}
