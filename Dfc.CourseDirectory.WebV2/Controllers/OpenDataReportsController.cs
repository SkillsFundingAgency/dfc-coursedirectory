using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Mvc;
using Dfc.CourseDirectory.Core.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dfc.CourseDirectory.Core.Extensions;

namespace Dfc.CourseDirectory.WebV2.Controllers
{
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
        public async Task<IActionResult> LiveCourseProviders()
        {
            var fromDate = _clock.UtcNow.AddMonths(-1).AddDays((_clock.UtcNow.Day - 1) * -1);

            return await _mediator.SendAndMapResponse(
                new ViewComponents.OpenData.Reporting.LiveCourseProvidersReport.Query
                {
                    FromDate = fromDate,
                }, // TODO: allow any cut-off date.
                records => new CsvResult<ViewComponents.OpenData.Reporting.LiveCourseProvidersReport.Csv>(
                    $"{nameof(ViewComponents.OpenData.Reporting.LiveCourseProvidersReport)}-{_clock.UtcNow:yyyyMMdd}.csv", records));
        }

        [HttpGet("live-courses-with-regions-and-venues-report")]
        public async Task<IActionResult> LiveCoursesWithRegionsAndVenues()
        {
            var fromDate = _clock.UtcNow.AddMonths(-1).AddDays((_clock.UtcNow.Day - 1) * -1);

            return await _mediator.SendAndMapResponse(
                new ViewComponents.OpenData.Reporting.LiveCoursesWithRegionsAndVenuesReport.Query
                {
                    FromDate = fromDate,
                }, // TODO: allow any cut-off date.
                records => new CsvResult<ViewComponents.OpenData.Reporting.LiveCoursesWithRegionsAndVenuesReport.Csv>(
                    $"{nameof(ViewComponents.OpenData.Reporting.LiveCoursesWithRegionsAndVenuesReport)}-{_clock.UtcNow:yyyyMMdd}.csv", records));
        }

        [HttpGet("live-regions-report")]
        public async Task<IActionResult> LiveRegions()
        {
            var fromDate = _clock.UtcNow.AddMonths(-1).AddDays((_clock.UtcNow.Day - 1) * -1);

            return await _mediator.SendAndMapResponse(
                new ViewComponents.OpenData.Reporting.LiveRegionsReport.Query(),
                records => new CsvResult<ViewComponents.OpenData.Reporting.LiveRegionsReport.Csv>(
                    $"{nameof(ViewComponents.OpenData.Reporting.LiveRegionsReport)}-{_clock.UtcNow:yyyyMMdd}.csv", records));
        }
    }
}
