using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.ApprenticeshipAssessment;
using Dfc.CourseDirectory.WebV2.Security;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA
{
    [Route("apprenticeship-qa/apprenticeship-assessment/{apprenticeshipId}")]
    [Authorize(Policy = AuthorizationPolicyNames.Admin)]
    [JourneyMetadata(
        journeyName: "apprenticeship-qa/apprenticeship-assessment",
        stateType: typeof(JourneyModel),
        appendUniqueKey: false,
        requestDataKeys: "apprenticeshipId")]
    public class ApprenticeshipAssessmentController : Controller
    {
        private readonly IMediator _mediator;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;
        private JourneyInstance<JourneyModel> _journeyInstance;

        public ApprenticeshipAssessmentController(
            IMediator mediator,
            JourneyInstanceProvider journeyInstanceProvider)
        {
            _mediator = mediator;
            _journeyInstanceProvider = journeyInstanceProvider;
            _journeyInstance = _journeyInstanceProvider.GetInstance<JourneyModel>();
        }

        [HttpGet("")]
        public async Task<IActionResult> Get(
            Guid apprenticeshipId,
            Query query,
            [FromServices] JourneyModelInitializer journeyModelInitializer)
        {
            _journeyInstance = await _journeyInstanceProvider.GetOrCreateInstanceAsync(
                () => journeyModelInitializer.Initialize(apprenticeshipId));

            return await _mediator.SendAndMapResponse(query, vm => View("ApprenticeshipAssessment", vm));
        }

        [HttpPost("")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Post(Guid apprenticeshipId, Command command) =>
            await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors("ApprenticeshipAssessment", errors),
                    vm => RedirectToAction(
                        nameof(GetConfirmation),
                        new { apprenticeshipId })));

        [HttpGet("confirmation")]
        [RequireJourneyInstance]
        public async Task<IActionResult> GetConfirmation(ConfirmationQuery query) =>
            await _mediator.SendAndMapResponse(query, vm => View("ApprenticeshipAssessmentConfirmation", vm));

        [HttpPost("confirmation")]
        [RequireJourneyInstance]
        public async Task<IActionResult> PostConfirmation(ConfirmationCommand command) =>
            await _mediator.SendAndMapResponse(
                command,
                success => RedirectToAction(
                    nameof(ApprenticeshipQAController.ProviderSelected),
                    "ApprenticeshipQA",
                    new { providerId = _journeyInstance.State.ProviderId }));
    }
}
