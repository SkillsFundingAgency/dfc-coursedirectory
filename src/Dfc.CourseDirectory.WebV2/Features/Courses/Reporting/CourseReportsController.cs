using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Mvc;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polly;
using GovUk.Frontend.AspNetCore;

namespace Dfc.CourseDirectory.WebV2.Features.Courses.Reporting
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
                new AllCoursesReport.Query(), 
                records => new CsvResult<AllCoursesReport.Csv>(
                    $"{nameof(AllCoursesReport)}-{_clock.UtcNow:yyyyMMdd}.csv", records));
        }
    }
}
