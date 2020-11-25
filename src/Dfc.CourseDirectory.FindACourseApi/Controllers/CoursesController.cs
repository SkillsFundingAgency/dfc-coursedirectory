using System.Threading.Tasks;
using Dfc.CourseDirectory.FindACourseApi.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.FindACourseApi.Controllers
{
    [ApiController]
    public class CoursesController : ControllerBase, IActionFilter
    {
        private readonly IMediator _mediator;
        private readonly ILogger _log;

        public CoursesController(
            IMediator mediator,
            ILogger<CoursesController> logger)
        {
            _mediator = mediator;
            _log = logger;
        }

        [Route("~/coursesearch")]
        [HttpPost]
        [ProducesResponseType(typeof(Features.CourseSearch.ViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CourseSearch([FromBody] Features.CourseSearch.Query request)
        {
            var response = await _mediator.Send(request);
            return new OkObjectResult(response);
        }

        [HttpGet("~/courserundetail")]
        [ProducesResponseType(typeof(CourseRunDetailViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CourseRunDetail([FromQuery] Features.CourseRunDetail.Query request)
        {
            var result = await _mediator.Send(request);

            if (result.Value is OneOf.Types.NotFound)
            {
                return NotFound();
            }

            return Ok(result.Value);
        }

        [NonAction]
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is ProblemDetailsException pde)
            {
                _log.LogInformation(
                    $"Request error on {context.ActionDescriptor.DisplayName}\nTitle: {pde.ProblemDetails.Title}\nDetail: {pde.ProblemDetails.Detail}");

                context.Result = new ObjectResult(pde.ProblemDetails)
                {
                    ContentTypes = new Microsoft.AspNetCore.Mvc.Formatters.MediaTypeCollection()
                    {
                        new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("application/problem+json")
                    },
                    StatusCode = pde.ProblemDetails.Status ?? 400
                };

                context.ExceptionHandled = true;
            }
        }

        [NonAction]
        public void OnActionExecuting(ActionExecutingContext context)
        {
        }
    }
}
