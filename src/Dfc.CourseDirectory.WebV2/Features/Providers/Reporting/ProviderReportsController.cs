using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Mvc;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Providers.Reporting
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
            _mediator.SendAndMapResponse<IAsyncEnumerable<ProviderTypeReport.Csv>, IActionResult>(new ProviderTypeReport.Query(),
                records => new CsvResult<ProviderTypeReport.Csv>($"{nameof(ProviderTypeReport)}-{_clock.UtcNow:yyyyMMddHHmmss}.csv", records));


        [HttpGet("providers-missing-primary-contact")]
        public Task<IActionResult> ProvidersMissingPrimaryContact() =>
            _mediator.SendAndMapResponse<IAsyncEnumerable<ProviderMissingPrimaryContactReport.Csv>, IActionResult>(new ProviderMissingPrimaryContactReport.Query(),
                records => new CsvResult<ProviderMissingPrimaryContactReport.Csv>($"{nameof(ProviderMissingPrimaryContactReport)}-{_clock.UtcNow:yyyyMMddHHmmss}.csv", records));
    }
}
