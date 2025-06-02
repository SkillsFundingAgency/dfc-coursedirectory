using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Extensions;

namespace Dfc.CourseDirectory.WebV2.Features.Courses
{
    [Route("courses/expired")]
    [RequireProviderContext]
    public class ExpiredCourseRunsController : Controller
    {
        private readonly IMediator _mediator;
        private ISession Session => HttpContext.Session;
        protected const string SessionNonLarsCourse = "NonLarsCourse";
        public ExpiredCourseRunsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(bool isNonLars)
        {
            if (isNonLars)
            {
                Session.SetString(SessionNonLarsCourse, "true");
            }
            else
            {
                Session.SetString(SessionNonLarsCourse, "false");
            }
            return await _mediator.SendAndMapResponse(new ExpiredCourseRuns.Query() { IsNonLars = isNonLars }, vm => View("ExpiredCourseRuns", vm));
        }

    }
}
