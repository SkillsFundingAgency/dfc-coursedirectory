using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Mvc;
using Dfc.CourseDirectory.Core.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dfc.CourseDirectory.Core.Extensions;

namespace Dfc.CourseDirectory.Web.Controllers.Providers
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
            _mediator.SendAndMapResponse<IAsyncEnumerable<ViewModels.Providers.Reporting.ProviderTypeReport.Csv>, IActionResult>(new ViewModels.Providers.Reporting.ProviderTypeReport.Query(),
                records => new CsvResult<ViewModels.Providers.Reporting.ProviderTypeReport.Csv>($"{nameof(ProviderTypeReport)}-{_clock.UtcNow:yyyyMMddHHmmss}.csv", records));


        [HttpGet("providers-missing-primary-contact")]
        public Task<IActionResult> ProvidersMissingPrimaryContact() =>
            _mediator.SendAndMapResponse<IAsyncEnumerable<ViewModels.Providers.Reporting.ProviderMissingPrimaryContactReport.Csv>, IActionResult>(new ViewModels.Providers.Reporting.ProviderMissingPrimaryContactReport.Query(),
                records => new CsvResult<ViewModels.Providers.Reporting.ProviderMissingPrimaryContactReport.Csv>($"{nameof(ViewModels.Providers.Reporting.ProviderMissingPrimaryContactReport)}-{_clock.UtcNow:yyyyMMddHHmmss}.csv", records));
    }
}
