using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.ViewAndEditTLevel
{
    [Route("t-levels/{tLevelId}")]
    [RequireFeatureFlag(FeatureFlags.TLevels)]
    [RestrictProviderTypes(ProviderType.TLevels)]
    [AuthorizeTLevel]
    public class ViewTLevelController : Controller
    {
        private readonly IMediator _mediator;

        public ViewTLevelController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromRoute] ViewTLevel.Query query) =>
            await _mediator.SendAndMapResponse(
                query,
                r => r.Match<IActionResult>(
                    _ => NotFound(),
                    vm => View("ViewTLevel", vm)));
    }
}
