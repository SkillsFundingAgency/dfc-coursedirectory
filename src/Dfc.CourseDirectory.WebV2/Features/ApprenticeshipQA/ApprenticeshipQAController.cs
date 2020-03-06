using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> ListProviders()
        {
            var result = await _mediator.Send(new ListProviders.Query());
            return View(result);
        }

        [HttpGet("{providerId}")]
        public IActionResult ProviderDetail(Guid providerId) =>
            throw new System.NotImplementedException();

        [HttpGet("{providerId}/provider-assessment")]
        public async Task<IActionResult> ProviderAssessment(ProviderAssessment.Query query)
        {
            var result = await _mediator.Send(query);
            return result.Match<IActionResult>(
                error => BadRequest(),
                vm => View(vm));
        }

        [HttpPost("{providerId}/provider-assessment")]
        public async Task<IActionResult> ProviderAssessment(
            Guid providerId,
            ProviderAssessment.Command command)
        {
            command.ProviderId = providerId;
            var result = await _mediator.Send(command);
            return result.Match(
                vm => View("ProviderAssessmentConfirmation", vm),
                errors => this.ViewFromErrors(errors));
        }
    }
}
