using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.WebV2.Features.Providers
{
    [Route("providers/provider-type")]
    [RequireProviderContext]
    public class EditProviderTypeController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly ProviderContext _providerContext;

        public EditProviderTypeController(
            IMediator mediator,
            ICurrentUserProvider currentUserProvider,
            IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _currentUserProvider = currentUserProvider;
            _providerContext = providerContextProvider.GetProviderContext();
        }

        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            var query = new EditProviderType.Query() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View("EditProviderType", vm));
        }

        [HttpPost("")]
        public async Task<IActionResult> Post(EditProviderType.Command command)
        {
            if (command.ProviderId != _providerContext.ProviderInfo.ProviderId)
            {
                return BadRequest();
            }

            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors("EditProviderType", errors),
                    confirmErrors => this.ViewFromErrors("EditProviderTypeConfirm", confirmErrors),
                    confirm => View("EditProviderTypeConfirm", confirm),
                    cancel => RedirectToAction(nameof(Get)).WithProviderContext(_providerContext),
                    success => RedirectToAction("ProviderDetails", "Providers").WithProviderContext(_providerContext)));
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
