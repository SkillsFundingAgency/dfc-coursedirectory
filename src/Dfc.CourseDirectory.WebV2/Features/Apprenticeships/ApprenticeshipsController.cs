using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Apprenticeships
{
    [Route("apprenticeships")]
    public class ApprenticeshipsController : Controller
    {
        private const string FindStandardOrFrameworkFlowName = nameof(FindStandardOrFramework);
        
        private readonly IMediator _mediator;

        public ApprenticeshipsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("find-standard")]
        public async Task<IActionResult> FindStandardOrFramework(
            [LocalUrl(viewDataKey: "ReturnUrl")] string returnUrl,
            ProviderInfo providerInfo)
        {
            var query = new FindStandardOrFramework.Query() { ProviderId = providerInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, response => View(response));
        }   

        [HttpGet("find-standard/search")]
        public async Task<IActionResult> FindStandardOrFrameworkSearch(
            FindStandardOrFramework.SearchQuery query,
            [LocalUrl(viewDataKey: "ReturnUrl")] string returnUrl,
            ProviderInfo providerInfo)
        {
            query.ProviderId = providerInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                query,
                response => response.Match(
                    errors => this.ViewFromErrors("FindStandardOrFramework", errors),
                    vm => View("FindStandardOrFramework", vm)));
        }
    }
}
