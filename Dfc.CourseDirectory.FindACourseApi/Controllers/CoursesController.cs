using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.FindACourseApi.Controllers
{
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CoursesController> _log;
        public CoursesController(IMediator mediator, ILogger<CoursesController> log)
        {
            _mediator = mediator;
            _log = log;
        }

        [HttpGet("~/courserundetail")]
        [ProducesResponseType(typeof(Features.CourseRunDetail.CourseRunDetailViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CourseRunDetail([FromQuery] Features.CourseRunDetail.Query request)
        {
            _log.LogInformation("Start Getting Course Run Details for [{CourseRunId}] in course [{CourseId}]",request.CourseRunId, request.CourseId);

            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                _ =>
                {
                    _log.LogWarning("Failed to get Course Run Detail for [{CourseRunId}] in course [{.CourseId}]. Response Code [NOT FOUND]", request.CourseRunId, request.CourseId);
                    return NotFound();
                },
                r =>
                {
                    _log.LogInformation("Successfully retrieved Course Run Detail for [{CourseRunId}] in course [{CourseId}]. Response Code [OK]", request.CourseRunId, request.CourseId);
                    return Ok(r);
                }
            );
        }
    }
}
