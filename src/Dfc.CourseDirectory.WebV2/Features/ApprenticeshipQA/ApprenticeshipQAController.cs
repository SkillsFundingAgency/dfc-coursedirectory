using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Helpers;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA
{
    [Route("apprenticeship-qa")]
    [Authorize(Policy = AuthorizationPolicyNames.ApprenticeshipQA)]
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

        [HttpGet("{providerId}/apprenticeship-assessment/{apprenticeshipId}")]
        public async Task<IActionResult> ApprenticeshipAssessmentStart(
            [ApprenticeshipId(DoesNotExistResponseStatusCode = 400)] Guid apprenticeshipId,
            [FromServices] MptxManager mptxManager,
            [FromServices] ApprenticeshipAssessment.FlowModelInitializer flowModelInitializer)
        {
            var flowModel = await flowModelInitializer.Initialize(apprenticeshipId);
            var flow = mptxManager.CreateInstance(flowModel);
            return RedirectToAction(nameof(ApprenticeshipAssessment))
                .WithMptxInstanceId(flow);
        }

        [MptxAction]
        [HttpGet("apprenticeship-assessment")]
        public async Task<IActionResult> ApprenticeshipAssessment(ApprenticeshipAssessment.Query query) =>
            await _mediator.SendAndMapResponse(query, vm => View(vm));

        [MptxAction]
        [HttpPost("apprenticeship-assessment")]
        public async Task<IActionResult> ApprenticeshipAssessment(
            ApprenticeshipAssessment.Command command,
            MptxInstanceContext<ApprenticeshipAssessment.FlowModel> flow)
        {
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    vm => RedirectToAction(nameof(ApprenticeshipAssessmentConfirmation))
                        .WithMptxInstanceId(flow.InstanceId)));
        }

        [MptxAction]
        [HttpGet("apprenticeship-assessment-confirmation")]
        public async Task<IActionResult> ApprenticeshipAssessmentConfirmation(ApprenticeshipAssessment.ConfirmationQuery query) =>
            await _mediator.SendAndMapResponse(query, vm => View(vm));

        [MptxAction]
        [HttpPost("apprenticeship-assessment-confirmation")]
        public async Task<IActionResult> ApprenticeshipAssessmentConfirmation(
            ApprenticeshipAssessment.ConfirmationCommand command,
            MptxInstanceContext<ApprenticeshipAssessment.FlowModel> flow)
        {
            return await _mediator.SendAndMapResponse(
                command,
                success => RedirectToAction(nameof(ProviderSelected), new { providerId = flow.State.ProviderId }));
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
