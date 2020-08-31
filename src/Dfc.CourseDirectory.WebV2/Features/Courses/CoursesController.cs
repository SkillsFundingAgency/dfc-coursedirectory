using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Courses
{
    [Route("courses/{courseId}/course-runs/{courseRunId}")]
    public class CoursesController : Controller
    {
        private readonly IMediator _mediator;

        public CoursesController(IMediator mediator) => _mediator = mediator;

        [HttpGet("delete")]
        public async Task<IActionResult> DeleteCourseRun(
            DeleteCourseRun.Request request,
            [LocalUrl(viewDataKey: "ReturnUrl")] string returnUrl) =>
            await _mediator.SendAndMapResponse(
                request,
                vm => View(vm));

        [HttpPost("delete")]
        public Task<IActionResult> DeleteCourseRun(DeleteCourseRun.Command request) =>
            _mediator.SendAndMapResponse(
                request,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    vm => RedirectToAction(
                        "CourseConfirmationDelete",
                        "ProviderCourses",
                        new
                        {
                            courseRunId = vm.CourseRunId,
                            courseName = vm.CourseName
                        })));
    }
}
