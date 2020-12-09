﻿using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.ProviderAssessment;
using Dfc.CourseDirectory.WebV2.Security;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA
{
    [Route("apprenticeship-qa/provider-assessment/{providerId}")]
    [Authorize(Policy = AuthorizationPolicyNames.ApprenticeshipQA)]
    [JourneyMetadata(
        journeyName: "apprenticeship-qa/provider-assessment",
        stateType: typeof(JourneyModel),
        appendUniqueKey: false,
        requestDataKeys: "providerId")]
    public class ProviderAssessmentController : Controller
    {
        private readonly IMediator _mediator;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;
        private JourneyInstance<JourneyModel> _journeyInstance;

        public ProviderAssessmentController(
            IMediator mediator,
            JourneyInstanceProvider journeyInstanceProvider)
        {
            _mediator = mediator;
            _journeyInstanceProvider = journeyInstanceProvider;
            _journeyInstance = _journeyInstanceProvider.GetInstance<JourneyModel>();
        }

        [HttpGet("")]
        public async Task<IActionResult> Get(
            Guid providerId,
            Query query,
            [FromServices] JourneyModelInitializer journeyModelInitializer)
        {
            _journeyInstance = await _journeyInstanceProvider.GetOrCreateInstanceAsync(
                () => journeyModelInitializer.Initialize(providerId));

            return await _mediator.SendAndMapResponse(query, vm => View("ProviderAssessment", vm));
        }

        [HttpPost("")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Post(Guid providerId, Command command) =>
            await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors("ProviderAssessment", errors),
                    vm => RedirectToAction(
                        nameof(GetConfirmation),
                        new { providerId })));

        [HttpGet("confirmation")]
        [RequireJourneyInstance]
        public async Task<IActionResult> GetConfirmation(ConfirmationQuery query) =>
            await _mediator.SendAndMapResponse(query, vm => View("ProviderAssessmentConfirmation", vm));

        [HttpPost("confirmation")]
        [RequireJourneyInstance]
        public async Task<IActionResult> PostConfirmation(ConfirmationCommand command, Guid providerId) =>
            await _mediator.SendAndMapResponse(
                command,
                success => RedirectToAction(
                    nameof(ApprenticeshipQAController.ProviderSelected),
                    "ApprenticeshipQA",
                    new { providerId = providerId }));
    }
}
