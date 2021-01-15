using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
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
            var query = new DisplayName.Query() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [HttpPost("display-name")]
        public async Task<IActionResult> DisplayName(DisplayName.Command command)
        {
            command.ProviderId = _providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                success => RedirectToAction(nameof(ProviderDetails)).WithProviderContext(_providerContext));
        }

        [HttpGet("info")]
        [AuthorizeAdmin]
        [RestrictProviderTypes(ProviderType.Apprenticeships)]
        public async Task<IActionResult> EditProviderInfo()
        {
            var query = new EditProviderInfo.Query() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(
                query,
                command => View(command));
        }

        [HttpPost("info")]
        [AuthorizeAdmin]
        [RestrictProviderTypes(ProviderType.Apprenticeships)]
        public async Task<IActionResult> EditProviderInfo(EditProviderInfo.Command command)
        {
            command.ProviderId = _providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(ProviderDetails)).WithProviderContext(_providerContext)));
        }

        [HttpGet("")]
        public async Task<IActionResult> ProviderDetails()
        {
            var request = new ProviderDetails.Query() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(request, vm => View(vm));
        }
    }
}
