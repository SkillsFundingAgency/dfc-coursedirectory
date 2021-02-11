using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.ProviderSearch
{
    [AuthorizeAdmin]
    [Route("provider-search")]
    public class ProviderSearchController : Controller
    {
        private readonly IMediator _mediator;

        public ProviderSearchController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public Task<IActionResult> ProviderSearch([FromQuery] string searchQuery) =>
            _mediator.SendAndMapResponse<ViewModel, IActionResult>(
                new Query { SearchQuery = HttpContext.Request.Query.ContainsKey(nameof(searchQuery)) ? searchQuery ?? string.Empty : null },
                vm => View(vm));

        [HttpPost("onboard")]
        public Task<IActionResult> OnboardProvider(OnboardProviderCommand request) =>
            _mediator.SendAndMapResponse(
                request,
                result => result.Match<IActionResult>(
                    _ => NotFound(),
                    _ => RedirectToAction("Index", "ProviderDashboard", new { providerId = request.ProviderId })));
    }
}
