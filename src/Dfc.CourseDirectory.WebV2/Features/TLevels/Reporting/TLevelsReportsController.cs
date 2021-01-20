using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Mvc;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.Reporting
{
    [RequireFeatureFlag(FeatureFlags.TLevels)]
    [Authorize(Policy = AuthorizationPolicyNames.Admin)]
    [Route("t-levels/reports")]
    public class TLevelsReportsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IClock _clock;

        public TLevelsReportsController(IMediator mediator, IClock clock)
        {
            _mediator = mediator;
            _clock = clock;
        }

        [HttpGet("live-t-levels")]
        public async Task<IActionResult> LiveTLevelsReport() =>
            await _mediator.SendAndMapResponse(new LiveTLevelsReport.Query(),
                records => new CsvResult<LiveTLevelsReport.Csv>($"{nameof(LiveTLevelsReport)}-{_clock.UtcNow:yyyyMMddHHmmss}.csv", records));
    }
}
