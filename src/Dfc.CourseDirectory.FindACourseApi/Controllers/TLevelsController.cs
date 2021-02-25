using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.FindACourseApi.Controllers
{
    public class TLevelsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IFeatureFlagProvider _featureFlagProvider;

        public TLevelsController(IMediator mediator, IFeatureFlagProvider featureFlagProvider)
        {
            _mediator = mediator;
            _featureFlagProvider = featureFlagProvider;
        }

        [HttpGet("~/tleveldefinitions")]
        [ProducesResponseType(typeof(Features.TLevelDefinitions.ViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTLevelDefinitions()
        {
            return Ok(await _mediator.Send(new Features.TLevelDefinitions.Query()));
        }

        [HttpGet("~/tlevels")]
        [ProducesResponseType(typeof(Features.TLevels.ViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTLevels()
        {
            return Ok(await _mediator.Send(new Features.TLevels.Query()));
        }

        [HttpGet("~/tleveldetail")]
        [ProducesResponseType(typeof(Features.TLevels.TLevelDetailViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TLevelDetail([FromQuery] Features.TLevels.TLevelDetail.Query request)
        {
            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                _ => NotFound(),
                r => Ok(r));
        }
    }
}
