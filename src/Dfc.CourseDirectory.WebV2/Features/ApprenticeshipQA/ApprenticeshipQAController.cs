using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Helpers;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA
{
    [Route("apprenticeship-qa")]
    [Authorize(Policy = AuthorizationPolicyNames.ApprenticeshipQA)]
    [RequireFeatureFlag(FeatureFlags.ApprenticeshipQA)]
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

        [HttpGet("{providerId}/provider-assessment")]
        public async Task<IActionResult> ProviderAssessment(ProviderAssessment.Query query) =>
            await _mediator.SendAndMapResponse(query, vm => View(vm));

        [HttpPost("{providerId}/provider-assessment")]
        public async Task<IActionResult> ProviderAssessment(
            Guid providerId,
            ProviderAssessment.Command command)
        {
            command.ProviderId = providerId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match(
                    errors => this.ViewFromErrors(errors),
                    vm => View("ProviderAssessmentConfirmation", vm)));
        }

        [HttpGet("{providerId}/apprenticeship-assessments/{apprenticeshipId}")]
        public async Task<IActionResult> ApprenticeshipAssessment(
            [ApprenticeshipId(DoesNotExistResponseStatusCode = 400)] Guid apprenticeshipId,
            ProviderInfo providerInfo)
        {
            var query = new ApprenticeshipAssessment.Query()
            {
                ApprenticeshipId = apprenticeshipId,
                ProviderId = providerInfo.ProviderId
            };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [HttpPost("{providerId}/apprenticeship-assessments/{apprenticeshipId}")]
        public async Task<IActionResult> ApprenticeshipAssessment(
            [ApprenticeshipId(DoesNotExistResponseStatusCode = 400)] Guid apprenticeshipId,
            ProviderInfo providerInfo,
            ApprenticeshipAssessment.Command command)
        {
            command.ApprenticeshipId = apprenticeshipId;
            command.ProviderId = providerInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match(
                    errors => this.ViewFromErrors(errors),
                    vm => View("ApprenticeshipAssessmentConfirmation", vm)));
        }

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
        public async Task<IActionResult> Report() => await _mediator.SendAndMapResponse(
            new Report.Query(),
            response =>
            {
                var bytes = ReportHelper.ConvertToBytes(response);
                return File(bytes, "text/csv", "QAReport.csv");
            });
    }
}
