using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Courses
{
    [Route("courses/expired")]
    [RequireProviderContext]
    public class ExpiredCourseRunsController : Controller
    {
        private readonly IMediator _mediator;

        public ExpiredCourseRunsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index() =>
            await _mediator.SendAndMapResponse(new ExpiredCourseRuns.Query(), vm => View("ExpiredCourseRuns", vm));

        [HttpGet("courses/expired/SelectedCourses")]
        public IActionResult SelectedCourses()
        {
            return View("SelectedExpiredCourseRuns");
        }

        [HttpGet("courses/expired/SelectedCourses/updated")]
        public IActionResult UpdatedCourses()
        {
            return View("updated");
        }

    }
          
}
