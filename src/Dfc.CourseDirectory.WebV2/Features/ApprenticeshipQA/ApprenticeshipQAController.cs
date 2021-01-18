using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Mvc;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA
{
    [Route("apprenticeship-qa")]
    [Authorize(Policy = AuthorizationPolicyNames.Admin)]
    public class ApprenticeshipQAController : Controller
    {
        private readonly IMediator _mediator;

        public ApprenticeshipQAController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> ListProviders() =>
            await _mediator.SendAndMapResponse(new ListProviders.Query(), vm => View(vm));

        [HttpGet("{providerId}")]
        public async Task<IActionResult> ProviderSelected(ProviderSelected.Query query) =>
            await _mediator.SendAndMapResponse(query, vm => View(vm));

        [HttpPost("{providerId}/complete")]
        public async Task<IActionResult> Complete(Complete.Command command) =>
            await _mediator.SendAndMapResponse(command, vm => View(vm));

        [HttpGet("{providerId}/status")]
        public async Task<IActionResult> Status(Status.Query query) =>
            await _mediator.SendAndMapResponse(query, vm => View(vm));

        [HttpPost("{providerId}/status")]
        public async Task<IActionResult> Status(
            Guid providerId,
            Status.Command command)
        {
            command.ProviderId = providerId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    _ => RedirectToAction("ProviderSelected", new { providerId })));
        }

        [HttpGet("report")]
        public async Task<IActionResult> Report() =>
            await _mediator.SendAndMapResponse(new Report.Query(),
                response => new CsvResult<Report.ReportModel>("QAReport.csv", response.Report));
    }
}
