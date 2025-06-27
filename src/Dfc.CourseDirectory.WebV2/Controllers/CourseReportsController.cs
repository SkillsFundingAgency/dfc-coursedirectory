using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.Extensions;
using Dfc.CourseDirectory.Core.Security;
using Dfc.CourseDirectory.WebV2.Mvc;
using Dfc.CourseDirectory.WebV2.ViewModels.Courses.AllCoursesReport;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Controllers
{
    [Authorize(Policy = AuthorizationPolicyNames.Admin)]
    [Route("courses/reports")]
    public class CourseReportsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IClock _clock;

        public CourseReportsController(IMediator mediator, IClock clock)
        {
            _mediator = mediator;
            _clock = clock;
        }

        [HttpGet("all-courses")]
        public async Task<IActionResult> AllCourses()
        {

            return await _mediator.SendAndMapResponse(
                new Query(),
                records => new CsvResult<Csv>(
                    $"{nameof(ViewModels.Courses.AllCoursesReport)}-{_clock.UtcNow:yyyyMMdd}.csv", records));
        }
    }
}
