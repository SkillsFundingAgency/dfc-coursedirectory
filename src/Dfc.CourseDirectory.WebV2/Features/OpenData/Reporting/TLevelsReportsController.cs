using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Features.OpenData.Reporting.LiveCourseProvidersReport;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Mvc;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.OpenData.Reporting
{
    [RequireFeatureFlag(FeatureFlags.OpenData)]
    [Authorize(Policy = AuthorizationPolicyNames.Admin)]
    [Route("opendata/reports")]
    public class OpenDataReportsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IClock _clock;

        public OpenDataReportsController(IMediator mediator, IClock clock)
        {
            _mediator = mediator;
            _clock = clock;
        }

        [HttpGet("live-course-providers")]
        public async Task<IActionResult> LiveCourseProvidersReport() =>
            await _mediator.SendAndMapResponse(new LiveCourseProvidersReport.Query(),
                records => new CsvResult<Csv>($"{nameof(LiveCourseProvidersReport)}-{_clock.UtcNow:yyyyMMddHHmmss}.csv", records));
    }
}
