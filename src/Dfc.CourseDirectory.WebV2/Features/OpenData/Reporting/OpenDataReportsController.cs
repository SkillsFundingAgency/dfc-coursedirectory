using System;
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

        [HttpGet("live-course-providers-report")]
        public async Task<IActionResult> LiveCourseProviders() =>
            await _mediator.SendAndMapResponse(new LiveCourseProvidersReport.Query { FromDate = _clock.UtcNow.AddMonths(-1).AddDays((_clock.UtcNow.Day - 1)  * -1), }, // TODO: allow any cut-off date.
                records => new CsvResult<LiveCourseProvidersReport.Csv>($"{nameof(LiveCourseProvidersReport)}-{_clock.UtcNow:yyyyMMddHHmmss}.csv", records));

        [HttpGet("live-courses-with-regions-and-venues-report")]
        public async Task<IActionResult> LiveCoursesWithRegionsAndVenues() =>
            await _mediator.SendAndMapResponse(new LiveCoursesWithRegionsAndVenuesReport.Query { FromDate = _clock.UtcNow.AddMonths(-1).AddDays((_clock.UtcNow.Day - 1) * -1), }, // TODO: allow any cut-off date.
                records => new CsvResult<LiveCoursesWithRegionsAndVenuesReport.Csv>($"{nameof(LiveCoursesWithRegionsAndVenuesReport)}-{_clock.UtcNow:yyyyMMddHHmmss}.csv", records));
    }

}
