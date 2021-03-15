using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.ViewTLevels
{
    [RequireProviderContext]
    [RestrictProviderTypes(ProviderType.TLevels)]
    [Route("t-levels")]
    public class ViewTLevelsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ProviderContext _providerContext;

        public ViewTLevelsController(IMediator mediator, IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _providerContext = providerContextProvider.GetProviderContext();
        }

        [HttpGet("")]
        public async Task<IActionResult> List()
        {
            var query = new Query
            {
                ProviderId = _providerContext.ProviderInfo.ProviderId
            };

            return await _mediator.SendAndMapResponse(query, vm => View("ViewTLevels", vm));
        }
    }
}
