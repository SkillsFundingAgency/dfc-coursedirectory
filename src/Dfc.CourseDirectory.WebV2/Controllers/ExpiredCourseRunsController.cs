using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Attributes;
using Dfc.CourseDirectory.Core.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Controllers
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

            return await _mediator.SendAndMapResponse(
                new ViewModels.Courses.ExpiredCourseRuns.Query() { IsNonLars = isNonLars },
                vm => View("~/Views/Courses/ExpiredCourseRuns.cshtml", vm));
        }

    }
}
