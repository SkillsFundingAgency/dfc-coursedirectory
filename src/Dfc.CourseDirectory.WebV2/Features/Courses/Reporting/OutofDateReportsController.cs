using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Mvc;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Courses.Reporting
{
    [Authorize(Policy = AuthorizationPolicyNames.Admin)]
    [Route("courses/reports")]
    public class OutofDateReportsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IClock _clock;

        public OutofDateReportsController(IMediator mediator, IClock clock)
        {
            _mediator = mediator;
            _clock = clock;
        }

        [HttpGet("out-of-date-courses")]
        public async Task<IActionResult> OutofDateCoursesReport() =>
            await _mediator.SendAndMapResponse(new OutofDateCoursesReport.Query(),
                records => new CsvResult<OutofDateCoursesReport.Csv>(
                    $"{nameof(OutofDateCoursesReport)}-{_clock.UtcNow:yyyyMMddHHmmss}.csv", records));
    }
}
