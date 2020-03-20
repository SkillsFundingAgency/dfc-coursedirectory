using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Models;
using Flurl;
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

        [HttpGet("apprenticeship-details")]
        public async Task<IActionResult> ApprenticeshipDetails(
            StandardOrFramework standardOrFramework,
            ProviderInfo providerInfo)
        {
            var query = new ApprenticeshipDetails.Query()
            {
                ProviderId = providerInfo.ProviderId,
                StandardOrFramework = standardOrFramework
            };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [HttpPost("apprenticeship-details")]
        public async Task<IActionResult> ApprenticeshipDetails(
            ApprenticeshipDetails.Command command,
            StandardOrFramework standardOrFramework,
            ProviderInfo providerInfo)
        {
            command.ProviderId = providerInfo.ProviderId;
            command.StandardOrFramework = standardOrFramework;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction("ApprenticeshipLocations").WithCurrentProvider(providerInfo)));
        }

        [HttpGet("apprenticeship-locations")]
        public IActionResult ApprenticeshipLocations() => throw new System.NotImplementedException();

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
                    vm =>
                    {
                        var returnUrl = new Url(Url.Action("ApprenticeshipDetails")).WithProviderContext(providerInfo);
                        return RedirectToAction(
                            "FindStandardOrFramework",
                            "Apprenticeships",
                            new { returnUrl });
                    }));
        }
    }
}
