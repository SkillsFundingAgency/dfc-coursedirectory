using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.ViewTLevels
{
    [RequireFeatureFlag(FeatureFlags.TLevels)]
    [RestrictProviderTypes(ProviderType.TLevels)]
    [Route("t-levels")]
    public class ViewTLevelsController : Controller
    {
        private readonly IMediator _mediator;

        public ViewTLevelsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [RequireProviderContext]
        [HttpGet("")]
        public async Task<IActionResult> List(ProviderContext providerContext)
        {
            var query = new Query
            {
                ProviderId = providerContext.ProviderInfo.ProviderId
            };

            return await _mediator.SendAndMapResponse(query, vm => View("ViewTLevels", vm));
        }
    }
}
