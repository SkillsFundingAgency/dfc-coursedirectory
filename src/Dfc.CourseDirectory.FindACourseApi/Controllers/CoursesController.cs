using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.FindACourseApi.Controllers
{
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CoursesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("~/courserundetail")]
        [ProducesResponseType(typeof(Features.CourseRunDetail.CourseRunDetailViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CourseRunDetail([FromQuery] Features.CourseRunDetail.Query request)
        {
            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                _ => NotFound(),
                r => Ok(r));
        }
    }
}
