using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Dfc.CourseDirectory.Api.Controllers
{
    [ApiController]
    public class FindACourseController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FindACourseController> _log;

        public FindACourseController(IMediator mediator, ILogger<FindACourseController> log)
        {
            _mediator = mediator;
            _log = log;
        }

        [HttpPost("~/public/fac/search")]
        [ProducesResponseType(typeof(FindACourseApi.Features.Search.SearchViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromBody] FindACourseApi.Features.Search.Query request)
        {
            _log.LogInformation($"Start Searching for Courses by the Search Criteria [{request}]");
            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                p =>
                {
                    _log.LogWarning($"Failed to retrieve courses with given search criteria. {nameof(p.Title)}: {{{nameof(p.Title)}}}, {nameof(p.Detail)}: {{{nameof(p.Detail)}}}.", p.Title, p.Detail);

                    return new ObjectResult(p)
                    {
                        ContentTypes = new MediaTypeCollection()
                        {
                            new MediaTypeHeaderValue("application/problem+json")
                        },
                        StatusCode = p.Status ?? StatusCodes.Status400BadRequest
                    };
                },
                r =>
                {
                    _log.LogInformation($"Courses found. Returning Courses data in Json format. Response Code [OK]");
                    return Ok(r);
                });
        }

        [HttpGet("~/public/fac/courserundetail")]
        [ProducesResponseType(typeof(FindACourseApi.Features.CourseRunDetail.CourseRunDetailViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CourseRunDetail([FromQuery] FindACourseApi.Features.CourseRunDetail.Query request)
        {
            _log.LogInformation($"Start Getting for CourseRunDetail by the CourseId [{request.CourseId}] and CourseRunId [{request.CourseRunId}]");
            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                _ => {
                    _log.LogWarning($"Failed to retrieve CourseRun Details with given search criteria.Response Code [NOT FOUND]"); 
                    return NotFound();
                },
                r => {
                    _log.LogInformation($"CourseRun Details found. Returning data in Json format. Response Code [OK]");
                    return Ok(r); });
        }

        [HttpGet("~/public/fac/tleveldetail")]
        [ProducesResponseType(typeof(FindACourseApi.Features.TLevels.TLevelDetailViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TLevelDetail([FromQuery] FindACourseApi.Features.TLevels.TLevelDetail.Query request)
        {
            _log.LogInformation($"Start Getting for TLevel Detail by the TlevelID [{request.TLevelId}]");
            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                _ => {
                    _log.LogWarning($"Failed to retrieve TLevel Details with given T-Level ID.Response Code [NOT FOUND]");
                    return NotFound();
                },
                r => {
                    _log.LogInformation($"T-Level Details found. Returning data in Json format. Response Code [OK]");
                    return Ok(r);
                });
        }
    }
}
