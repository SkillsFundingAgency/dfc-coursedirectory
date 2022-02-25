using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Dfc.CourseDirectory.FindACourseApi.Controllers
{
    public class SearchController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SearchController> _log;
        private readonly TelemetryClient _telemetryClient;

        public SearchController(IMediator mediator, ILogger<SearchController> log, TelemetryClient telemetryClient)
        {
            _mediator = mediator;
            _log = log;
            _telemetryClient = telemetryClient;
        }

        [HttpPost("~/coursesearch")]  // Kept around to avoid breaking API change
        [HttpPost("~/search")]
        [ProducesResponseType(typeof(Features.Search.SearchViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CourseSearch([FromBody] Features.Search.Query request)
        {
            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                p =>
                {
                    _log.LogWarning($"{nameof(CourseSearch)} failed. {nameof(p.Title)}: {{{nameof(p.Title)}}}, {nameof(p.Detail)}: {{{nameof(p.Detail)}}}.", p.Title, p.Detail);

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
    }
}
