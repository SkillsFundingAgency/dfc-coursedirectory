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
    [Authorize(Policy = AuthorizationPolicyNames.ApprenticeshipQA)]
    [FormFlowAction(key: "apprenticeship-qa/apprenticeship-assessment", stateType: typeof(FlowModel), idRouteParameterNames: "apprenticeshipId")]
    public class ApprenticeshipAssessmentController : Controller
    {
        private readonly IMediator _mediator;

        public ApprenticeshipAssessmentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> Get(
            Guid apprenticeshipId,
            Query query,
            FormFlowInstanceFactory formFlowInstanceFactory,
            [FromServices] FlowModelInitializer flowModelInitializer)
        {
            await formFlowInstanceFactory.GetOrCreateInstanceAsync(
                () => flowModelInitializer.Initialize(apprenticeshipId));

            return await _mediator.SendAndMapResponse(query, vm => View("ApprenticeshipAssessment", vm));
        }

        [HttpPost("")]
        public async Task<IActionResult> Post(Command command, FormFlowInstance formFlowInstance) =>
            await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors("ApprenticeshipAssessment", errors),
                    vm => RedirectToAction(nameof(GetConfirmation))
                        .WithFormFlowInstanceId(formFlowInstance)));

        [HttpGet("confirmation")]
        public async Task<IActionResult> GetConfirmation(ConfirmationQuery query) =>
            await _mediator.SendAndMapResponse(query, vm => View("ApprenticeshipAssessmentConfirmation", vm));

        [HttpPost("confirmation")]
        public async Task<IActionResult> PostConfirmation(
            ConfirmationCommand command,
            FormFlowInstance<FlowModel> formFlowInstance) =>
            await _mediator.SendAndMapResponse(
                command,
                success => RedirectToAction(
                    nameof(ApprenticeshipQAController.ProviderSelected),
                    "ApprenticeshipQA",
                    new { providerId = formFlowInstance.State.ProviderId }));
    }
}
