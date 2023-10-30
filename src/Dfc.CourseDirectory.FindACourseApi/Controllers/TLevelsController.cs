using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.FindACourseApi.Controllers
{
    public class TLevelsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TLevelsController> _log;

        public TLevelsController(IMediator mediator, ILogger<TLevelsController> log)
        {
            _mediator = mediator;
            _log = log;
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
            _log.LogInformation($"Start Getting T-Level Details for [{request.TLevelId}]");

            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                _ => {
                    _log.LogWarning($"Failed to get T-Level Details for [{request.TLevelId}]. Response Code [NOT FOUND]");
                    return NotFound();
                },
                r => {
                    _log.LogInformation($"Successfully retrieved T-Level Details for [{request.TLevelId}]. Response Code [OK]");
                    return Ok(r);
                }
            );
        }
    }
}
