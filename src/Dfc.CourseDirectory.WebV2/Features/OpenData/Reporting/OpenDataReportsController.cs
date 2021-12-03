using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Mvc;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.OpenData.Reporting
{
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
                new LiveCourseProvidersReport.Query
                {
                    FromDate = _clock.UtcNow.AddMonths(-1).AddDays((_clock.UtcNow.Day - 1) * -1),
                }, // TODO: allow any cut-off date.
                records => new CsvResult<LiveCourseProvidersReport.Csv>(
                    $"{nameof(LiveCourseProvidersReport)}-{_clock.UtcNow:yyyyMMddHHmmss}.csv", records));
        }


        [HttpGet("live-courses-with-regions-and-venues-report")]
        public async Task<IActionResult> LiveCoursesWithRegionsAndVenues()
        {
            var fromDate = _clock.UtcNow.AddMonths(-1).AddDays((_clock.UtcNow.Day - 1) * -1);

            return await _mediator.SendAndMapResponse(
                new LiveCoursesWithRegionsAndVenuesReport.Query
                {
                    FromDate = fromDate,
                }, // TODO: allow any cut-off date.
                records => new CsvResult<LiveCoursesWithRegionsAndVenuesReport.Csv>(
                    $"{nameof(LiveCoursesWithRegionsAndVenuesReport)}-{_clock.UtcNow:yyyyMMddHHmmss}.csv", records));
        }

        [HttpGet("live-regions-report")]
        public async Task<IActionResult> LiveRegions()
        {
            var fromDate = _clock.UtcNow.AddMonths(-1).AddDays((_clock.UtcNow.Day - 1) * -1);

            return await _mediator.SendAndMapResponse(
                new LiveRegionsReport.Query(),
                records => new CsvResult<LiveRegionsReport.Csv>(
                    $"{nameof(LiveRegionsReport)}-{_clock.UtcNow:yyyyMMddHHmmss}.csv", records));
        }
    }
}
