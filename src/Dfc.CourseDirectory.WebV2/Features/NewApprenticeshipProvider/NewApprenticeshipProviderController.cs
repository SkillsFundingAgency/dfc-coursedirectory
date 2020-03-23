using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Flurl;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    [Route("new-apprenticeship-provider")]
    [RequireFeatureFlag(FeatureFlags.ApprenticeshipQA)]
    public class NewApprenticeshipProviderController : Controller
    {
        private const string FlowName = "NewApprenticeshipProvider";

        private readonly IMediator _mediator;

        public NewApprenticeshipProviderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [MptxAction(FlowName)]
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

        [MptxAction(FlowName)]
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

        [StartsMptx(FlowName, typeof(FlowModel))]
        [HttpGet("provider-detail")]
        public async Task<IActionResult> ProviderDetail(ProviderInfo providerInfo)
        {
            var query = new ProviderDetail.Query() { ProviderId = providerInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [MptxAction(FlowName)]
        [HttpPost("provider-detail")]
        public async Task<IActionResult> ProviderDetail(
            ProviderInfo providerInfo,
            MptxInstanceContext<FlowModel> flow,
            ProviderDetail.Command command)
        {
            command.ProviderId = providerInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(
                        "FindStandardOrFramework",
                        "Apprenticeships",
                        new
                        {
                            returnUrl = new Url(Url.Action("ApprenticeshipDetails"))
                                .WithProviderContext(providerInfo)
                                .WithMptxInstanceId(flow)
                        })));
        }
    }
}
