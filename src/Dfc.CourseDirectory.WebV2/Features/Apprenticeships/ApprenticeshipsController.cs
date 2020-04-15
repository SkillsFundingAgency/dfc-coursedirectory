using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Apprenticeships
{
    [Route("apprenticeships")]
    public class ApprenticeshipsController : Controller
    {
        private readonly IMediator _mediator;

        public ApprenticeshipsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [MptxAction]
        [HttpGet("classroom-location")]
        public async Task<IActionResult> ClassroomLocation(
            ClassroomLocation.Query query,
            [FromServices] MptxInstanceContext<ClassroomLocation.FlowModel, ClassroomLocation.IFlowModelCallback> flow)
        {
            return await _mediator.SendAndMapResponse(
                query,
                vm => View(vm).WithViewData("CompletionUrl", flow.Items["ReturnUrl"]));
        }

        [MptxAction]
        [HttpPost("classroom-location")]
        public async Task<IActionResult> ClassroomLocation(
            ClassroomLocation.Command command,
            [FromServices] MptxInstanceContext<ClassroomLocation.FlowModel, ClassroomLocation.IFlowModelCallback> flow)
        {
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors).WithViewData("CompletionUrl", flow.Items["ReturnUrl"]),
                    success => Redirect(flow.Items["ReturnUrl"].ToString())));
        }

        [HttpGet("find-standard")]
        public async Task<IActionResult> FindStandardOrFramework(
            [LocalUrl(viewDataKey: "ReturnUrl")] string returnUrl,
            ProviderInfo providerInfo)
        {
            var query = new FindStandardOrFramework.Query() { ProviderId = providerInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, response => View(response));
        }

        [HttpGet("find-standard/search")]
        public async Task<IActionResult> FindStandardOrFrameworkSearch(
            FindStandardOrFramework.SearchQuery query,
            [LocalUrl(viewDataKey: "ReturnUrl")] string returnUrl,
            ProviderInfo providerInfo)
        {
            query.ProviderId = providerInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                query,
                response => response.Match(
                    errors => this.ViewFromErrors("FindStandardOrFramework", errors),
                    vm => View("FindStandardOrFramework", vm)));
        }
    }
}
