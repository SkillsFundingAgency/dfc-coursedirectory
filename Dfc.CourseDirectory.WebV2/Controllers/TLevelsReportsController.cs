using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.Extensions;
using Dfc.CourseDirectory.Core.Security;
using Dfc.CourseDirectory.WebV2.Mvc;
using Dfc.CourseDirectory.WebV2.Reporting.LiveTLevelsReport;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Controllers
{
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
            await _mediator.SendAndMapResponse(new Query(),
                records => new CsvResult<Csv>($"{nameof(LiveTLevelsReport)}-{_clock.UtcNow:yyyyMMddHHmmss}.csv",
                    records));
    }
}
