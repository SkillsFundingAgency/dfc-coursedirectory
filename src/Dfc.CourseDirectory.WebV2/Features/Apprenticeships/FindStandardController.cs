using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Apprenticeships
{
    [Route("apprenticeships/find-standard")]
    public class FindStandardController : Controller
    {
        private readonly IMediator _mediator;

        public FindStandardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> Get(
            [LocalUrl(viewDataKey: "ReturnUrl")] string returnUrl,
            ProviderContext providerContext)
        {
            var query = new FindStandard.Query() { ProviderId = providerContext.ProviderInfo.ProviderId };

            return await _mediator.SendAndMapResponse(
                query,
                response => View("FindStandard", response));
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            FindStandard.SearchQuery query,
            ProviderContext providerContext,
            [LocalUrl(viewDataKey: "ReturnUrl")] string returnUrl)
        {
            query.ProviderId = providerContext.ProviderInfo.ProviderId;

            return await _mediator.SendAndMapResponse(
                query,
                response => response.Match(
                    errors => this.ViewFromErrors("FindStandard", errors),
                    vm => View("FindStandard", vm)));
        }
    }
}
