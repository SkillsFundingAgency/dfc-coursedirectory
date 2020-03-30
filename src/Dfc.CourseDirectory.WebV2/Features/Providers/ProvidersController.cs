using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Providers
{
    [Route("providers")]
    public class ProvidersController : Controller, IRequiresProviderContextController
    {
        private readonly IMediator _mediator;

        public ProvidersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public ProviderInfo ProviderContext { get; set; }

        [HttpGet("info")]
        public async Task<IActionResult> EditProviderInfo()
        {
            var query = new EditProviderInfo.Query() { ProviderId = ProviderContext.ProviderId };
            return await _mediator.SendAndMapResponse(
                query,
                response => response.Match(
                    errors => this.ViewFromErrors(errors, statusCode: System.Net.HttpStatusCode.OK),
                    vm => View(vm)));
        }

        [HttpPost("info")]
        public async Task<IActionResult> EditProviderInfo(EditProviderInfo.Command command)
        {
            command.ProviderId = ProviderContext.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction("Details", "Provider").WithProviderContext(ProviderContext)));
        }
    }
}
