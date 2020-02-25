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
        [HttpGet("info")]
        public async Task<IActionResult> EditProviderInfo(ProviderInfo providerInfo, [FromServices] IMediator mediator)
        {
            var request = new EditProviderInfo.Query() { Ukprn = providerInfo.Ukprn };
            var response = await mediator.Send(request);
            return this.ViewFromErrors(response, statusCode: System.Net.HttpStatusCode.OK);
        }

        [HttpPost("info")]
        public async Task<IActionResult> EditProviderInfo(
            ProviderInfo providerInfo,
            [FromForm] EditProviderInfo.Command command,
            [FromServices] IMediator mediator)
        {
            command.Ukprn = providerInfo.Ukprn;
            return (await mediator.Send(command)).Match<IActionResult>(
                success => RedirectToAction("Details", "Provider").WithCurrentProvider(providerInfo),
                failed => this.ViewFromErrors(failed));
        }
    }
}
