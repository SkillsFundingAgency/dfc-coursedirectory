using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.Extensions;
using Dfc.CourseDirectory.Core.Security;
using Dfc.CourseDirectory.WebV2.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Controllers
{
    [Authorize(Policy = AuthorizationPolicyNames.Admin)]
    [Route("providers/reports")]
    public class ProviderReportsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IClock _clock;

        public ProviderReportsController(IMediator mediator, IClock clock)
        {
            _mediator = mediator;
            _clock = clock;
        }

        [HttpGet("provider-type")]
        public Task<IActionResult> ProviderTypeReport() =>
            _mediator.SendAndMapResponse<IAsyncEnumerable<Reporting.ProviderTypeReport.Csv>, IActionResult>(new Reporting.ProviderTypeReport.Query(),
                records => new CsvResult<Reporting.ProviderTypeReport.Csv>($"{nameof(ProviderTypeReport)}-{_clock.UtcNow:yyyyMMddHHmmss}.csv", records));


        [HttpGet("providers-missing-primary-contact")]
        public Task<IActionResult> ProvidersMissingPrimaryContact() =>
            _mediator.SendAndMapResponse<IAsyncEnumerable<Reporting.ProviderMissingPrimaryContactReport.Csv>, IActionResult>(new Reporting.ProviderMissingPrimaryContactReport.Query(),
                records => new CsvResult<Reporting.ProviderMissingPrimaryContactReport.Csv>($"{nameof(Reporting.ProviderMissingPrimaryContactReport)}-{_clock.UtcNow:yyyyMMddHHmmss}.csv", records));
    }
}
