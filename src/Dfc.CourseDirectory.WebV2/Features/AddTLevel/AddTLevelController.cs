using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Filters;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.AddTLevel
{
    [Route("t-levels/add")]
    [JourneyMetadata(
        journeyName: "AddTLevel",
        stateType: typeof(AddTLevelJourneyModel),
        appendUniqueKey: true,
        requestDataKeys: "providerId?")]
    [RequireFeatureFlag(FeatureFlags.TLevels)]
    [RequireProviderContext]
    [RestrictProviderTypes(ProviderType.TLevels)]
    public class AddTLevelController : Controller
    {
        private readonly JourneyInstanceProvider _journeyInstanceProvider;
        private readonly IMediator _mediator;
        private JourneyInstance<AddTLevelJourneyModel> _journeyInstance;

        public AddTLevelController(
            JourneyInstanceProvider JourneyInstanceProvider,
            IMediator mediator)
        {
            _journeyInstanceProvider = JourneyInstanceProvider;
            _mediator = mediator;
            _journeyInstance = JourneyInstanceProvider.GetInstance<AddTLevelJourneyModel>();
        }

        [HttpGet("")]
        public async Task<IActionResult> SelectTLevel(ProviderContext providerContext)
        {
            _journeyInstance = _journeyInstanceProvider.GetOrCreateInstance(
                () => new AddTLevelJourneyModel());

            if (!_journeyInstanceProvider.IsCurrentInstance(_journeyInstance))
            {
                return RedirectToAction()
                    .WithProviderContext(providerContext)
                    .WithJourneyInstanceUniqueKey(_journeyInstance);
            }

            var query = new SelectTLevel.Query() { ProviderId = providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View("SelectTLevel", vm));
        }

        [HttpPost("")]
        [RequireJourneyInstance]
        public async Task<IActionResult> SelectTLevel(
            SelectTLevel.Command command,
            ProviderContext providerContext)
        {
            command.ProviderId = providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(Description))
                        .WithProviderContext(providerContext)
                        .WithJourneyInstanceUniqueKey(_journeyInstance)));
        }

        [HttpGet("description")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Description()
        {
            var query = new Description.Query();
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [HttpPost("description")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Description(
            Description.Command command,
            [FromQuery] bool? fromPublishPage,
            ProviderContext providerContext)
        {
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(fromPublishPage == true ? nameof(CheckAndPublish) : nameof(Details))
                        .WithProviderContext(providerContext)
                        .WithJourneyInstanceUniqueKey(_journeyInstance)));
        }

        [HttpGet("details")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Details(ProviderContext providerContext)
        {
            var query = new Details.Query() { ProviderId = providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [HttpPost("details")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Details(
            Details.Command command,
            ProviderContext providerContext)
        {
            command.ProviderId = providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(CheckAndPublish))
                        .WithProviderContext(providerContext)
                        .WithJourneyInstanceUniqueKey(_journeyInstance)));
        }

        [HttpGet("check-publish")]
        [RequireJourneyInstance]
        public async Task<IActionResult> CheckAndPublish()
        {
            var query = new CheckAndPublish.Query();
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [HttpPost("check-publish")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Publish(ProviderContext providerContext)
        {
            var command = new CheckAndPublish.Command() { ProviderId = providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(nameof(CheckAndPublish), errors),
                    success => RedirectToAction(nameof(Published))
                        .WithProviderContext(providerContext)
                        .WithJourneyInstanceUniqueKey(_journeyInstance)));
        }

        [HttpGet("success")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Published()
        {
            var query = new Published.Query();
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }
    }
}
