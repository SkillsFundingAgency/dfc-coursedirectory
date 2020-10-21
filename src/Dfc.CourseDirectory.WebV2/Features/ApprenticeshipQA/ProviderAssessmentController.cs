using System;
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
    [FormFlowAction(key: "apprenticeship-qa/provider-assessment", stateType: typeof(FlowModel), idRouteParameterNames: "providerId")]
    public class ProviderAssessmentController : Controller
    {
        private readonly IMediator _mediator;

        public ProviderAssessmentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> Get(
            Guid providerId,
            Query query,
            FormFlowInstanceFactory formFlowInstanceFactory,
            [FromServices] FlowModelInitializer flowModelInitializer)
        {
            await formFlowInstanceFactory.GetOrCreateInstanceAsync(
                () => flowModelInitializer.Initialize(providerId));

            return await _mediator.SendAndMapResponse(query, vm => View("ProviderAssessment", vm));
        }

        [HttpPost("")]
        [RequiresFormFlowInstance]
        public async Task<IActionResult> Post(Command command, FormFlowInstance formFlowInstance) =>
            await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors("ProviderAssessment", errors),
                    vm => RedirectToAction(nameof(GetConfirmation))
                        .WithFormFlowInstanceId(formFlowInstance)));

        [HttpGet("confirmation")]
        [RequiresFormFlowInstance]
        public async Task<IActionResult> GetConfirmation(ConfirmationQuery query) =>
            await _mediator.SendAndMapResponse(query, vm => View("ProviderAssessmentConfirmation", vm));

        [HttpPost("confirmation")]
        [RequiresFormFlowInstance]
        public async Task<IActionResult> PostConfirmation(ConfirmationCommand command, Guid providerId) =>
            await _mediator.SendAndMapResponse(
                command,
                success => RedirectToAction(
                    nameof(ApprenticeshipQAController.ProviderSelected),
                    "ApprenticeshipQA",
                    new { providerId = providerId }));
    }
}
