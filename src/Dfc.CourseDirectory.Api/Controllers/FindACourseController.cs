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

        [HttpPost("~/fac/search")]
        [ProducesResponseType(typeof(FindACourseApi.Features.Search.SearchViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromBody] FindACourseApi.Features.Search.Query request)
        {
            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                p =>
                {
                    _log.LogWarning($"{nameof(Search)} failed. {nameof(p.Title)}: {{{nameof(p.Title)}}}, {nameof(p.Detail)}: {{{nameof(p.Detail)}}}.", p.Title, p.Detail);

                    return new ObjectResult(p)
                    {
                        ContentTypes = new MediaTypeCollection()
                        {
                            new MediaTypeHeaderValue("application/problem+json")
                        },
                        StatusCode = p.Status ?? StatusCodes.Status400BadRequest
                    };
                },
                r => Ok(r));
        }

        [HttpGet("~/fac/courserundetail")]
        [ProducesResponseType(typeof(FindACourseApi.Features.CourseRunDetail.CourseRunDetailViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CourseRunDetail([FromQuery] FindACourseApi.Features.CourseRunDetail.Query request)
        {
            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                _ => NotFound(),
                r => Ok(r));
        }

        [HttpGet("~/fac/tleveldetail")]
        [ProducesResponseType(typeof(FindACourseApi.Features.TLevels.TLevelDetailViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TLevelDetail([FromQuery] FindACourseApi.Features.TLevels.TLevelDetail.Query request)
        {
            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                _ => NotFound(),
                r => Ok(r));
        }
    }
}
