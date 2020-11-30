using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.WebV2.Features.Providers
{
    [Route("providers/provider-type")]
    public class EditProviderTypeController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserProvider _currentUserProvider;

        public EditProviderTypeController(IMediator mediator, ICurrentUserProvider currentUserProvider)
        {
            _mediator = mediator;
            _currentUserProvider = currentUserProvider;
        }

        [HttpGet("")]
        public async Task<IActionResult> Get(ProviderContext providerContext)
        {
            var query = new EditProviderType.Query() { ProviderId = providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View("EditProviderType", vm));
        }

        [HttpPost("")]
        public async Task<IActionResult> Post(
            EditProviderType.Command command,
            ProviderContext providerContext)
        {
            command.ProviderId = providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                success => RedirectToAction("ProviderDetails", "Providers").WithProviderContext(providerContext));
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var currentUser = _currentUserProvider.GetCurrentUser();

            if (!AuthorizationRules.CanUpdateProviderType(currentUser))
            {
                throw new NotAuthorizedException();
            }
        }
    }
}
