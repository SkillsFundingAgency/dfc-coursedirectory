using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    [Route("new-apprenticeship-provider")]
    [RequireFeatureFlag(FeatureFlags.ApprenticeshipQA)]
    public class NewApprenticeshipProviderController : Controller
    {
        private readonly IMediator _mediator;

        public NewApprenticeshipProviderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public IActionResult Home(ProviderInfo providerInfo) =>
            RedirectToAction("MarketingInfo").WithCurrentProvider(providerInfo);

        [HttpGet("marketing-info")]
        public async Task<IActionResult> MarketingInfo(ProviderInfo providerInfo)
        {
            var query = new MarketingInfo.Query() { ProviderId = providerInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [HttpPost("marketing-info")]
        public async Task<IActionResult> MarketingInfo(
            ProviderInfo providerInfo,
            MarketingInfo.Command command)
        {
            command.ProviderId = providerInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    vm => RedirectToAction("SelectApprenticeshipFramework")));
        }

        [HttpGet("apprenticeship/framework")]
        public IActionResult SelectApprenticeshipFramework() => throw new NotImplementedException();
    }
}
