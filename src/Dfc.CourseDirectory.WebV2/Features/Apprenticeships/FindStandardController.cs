using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Apprenticeships
{
    [Route("apprenticeships/find-standard")]
    [RestrictProviderTypes(ProviderType.Apprenticeships)]
    [RequireProviderContext]
    public class FindStandardController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ProviderContext _providerContext;

        public FindStandardController(IMediator mediator, IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _providerContext = providerContextProvider.GetProviderContext();
        }

        [HttpGet("")]
        public async Task<IActionResult> Get(
            [LocalUrl(viewDataKey: "ReturnUrl")] string returnUrl)
        {
            var query = new FindStandard.Query()
            {
                ProviderId = _providerContext.ProviderInfo.ProviderId
            };

            return await _mediator.SendAndMapResponse(
                query,
                response => View("FindStandard", response));
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            FindStandard.SearchQuery query,
            [LocalUrl(viewDataKey: "ReturnUrl")] string returnUrl)
        {
            query.ProviderId = _providerContext.ProviderInfo.ProviderId;

            return await _mediator.SendAndMapResponse(
                query,
                response => response.Match(
                    errors => this.ViewFromErrors("FindStandard", errors),
                    vm => View("FindStandard", vm)));
        }
    }
}
