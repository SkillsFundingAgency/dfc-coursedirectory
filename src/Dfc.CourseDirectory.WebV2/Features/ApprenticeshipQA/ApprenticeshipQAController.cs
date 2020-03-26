﻿using Dfc.CourseDirectory.WebV2.Filters;
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

        [HttpGet("provider-assessments/{providerId}")]
        public async Task<IActionResult> ProviderAssessment(ProviderAssessment.Query query) =>
            await _mediator.SendAndMapResponse(query, vm => View(vm));

        [HttpPost("provider-assessments/{providerId}")]
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

        [HttpGet("apprenticeship-assessments/{apprenticeshipId}")]
        public async Task<IActionResult> ApprenticeshipAssessment(ApprenticeshipAssessment.Query query) =>
            await _mediator.SendAndMapResponse(query, vm => View(vm));

        [HttpPost("apprenticeship-assessments/{apprenticeshipId}")]
        public async Task<IActionResult> ApprenticeshipAssessment(
            Guid apprenticeshipId,
            ApprenticeshipAssessment.Command command)
        {
            command.ApprenticeshipId = apprenticeshipId;
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

        [HttpGet("qareport")]
        public async Task<IActionResult> Report() => await _mediator.SendAndMapResponse(
            new Report.Query(),
            response =>
            {
                var bytes = ReportHelper.ConvertToBytes(response);
                var file = File(bytes, "text/csv", "QAReport.csv");
                return file;
            });
    }
}
