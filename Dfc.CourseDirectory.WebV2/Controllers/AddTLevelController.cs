using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Attributes;
using Dfc.CourseDirectory.Core.Extensions;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.ViewModels.TLevels.AddTLevel;
using Flurl;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Controllers
{
    [Route("t-levels/add")]
    [JourneyMetadata(
        journeyName: "AddTLevel",
        stateType: typeof(AddTLevelJourneyModel),
        appendUniqueKey: true,
        requestDataKeys: "providerId?")]
    [RequireProviderContext]
    [RestrictProviderTypes(ProviderType.TLevels)]
    public class AddTLevelController : Controller
    {
        private readonly JourneyInstanceProvider _journeyInstanceProvider;
        private readonly IMediator _mediator;
        private JourneyInstance<AddTLevelJourneyModel> _journeyInstance;
        private readonly ProviderContext _providerContext;

        public AddTLevelController(
            JourneyInstanceProvider JourneyInstanceProvider,
            IMediator mediator,
            IProviderContextProvider providerContextProvider)
        {
            _journeyInstanceProvider = JourneyInstanceProvider;
            _mediator = mediator;
            _journeyInstance = JourneyInstanceProvider.GetInstance<AddTLevelJourneyModel>();
            _providerContext = providerContextProvider.GetProviderContext();
        }

        [HttpGet("")]
        public async Task<IActionResult> SelectTLevel()
        {
            _journeyInstance = _journeyInstanceProvider.GetOrCreateInstance(
                () => new AddTLevelJourneyModel());

            if (!_journeyInstanceProvider.IsCurrentInstance(_journeyInstance))
            {
                return RedirectToAction()
                    .WithProviderContext(_providerContext)
                    .WithJourneyInstanceUniqueKey(_journeyInstance);
            }

            var query = new ViewModels.TLevels.AddTLevel.SelectTLevel.Query() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(query, vm => View("SelectTLevel", vm));
        }

        [HttpPost("")]
        [RequireJourneyInstance]
        public async Task<IActionResult> SelectTLevel(ViewModels.TLevels.AddTLevel.SelectTLevel.Command command)
        {
            command.ProviderId = _providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(Description))
                        .WithProviderContext(_providerContext)
                        .WithJourneyInstanceUniqueKey(_journeyInstance)));
        }

        [HttpGet("description")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Description()
        {
            var query = new ViewModels.TLevels.AddTLevel.Description.Query();
            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) + "find-a-course/search";

            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [HttpPost("description")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Description(
            ViewModels.TLevels.AddTLevel.Description.Command command,
            [FromQuery] bool? fromPublishPage)
        {
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(fromPublishPage == true ? nameof(CheckAndPublish) : nameof(Details))
                        .WithProviderContext(_providerContext)
                        .WithJourneyInstanceUniqueKey(_journeyInstance)));
        }

        [HttpGet("details")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Details(
            [FromQuery] Guid? venueId)  // Populated by the Add Venue callback journey)
        {
            var query = new ViewModels.TLevels.AddTLevel.Details.Query() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(
                query,
                vm =>
                {
                    // If we've just added a new venue, ensure it's selected
                    if (venueId.HasValue && vm.ProviderVenues.Any(v => v.VenueId == venueId))
                    {
                        vm.LocationVenueIds.Add(venueId.Value);
                    }

                    return View(vm);
                });
        }

        [HttpPost("details")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Details(ViewModels.TLevels.AddTLevel.Details.Command command)
        {
            command.ProviderId = _providerContext.ProviderInfo.ProviderId;
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(CheckAndPublish))
                        .WithProviderContext(_providerContext)
                        .WithJourneyInstanceUniqueKey(_journeyInstance)));
        }

        [HttpPost("add-location")]
        public async Task<IActionResult> AddAnotherLocation(ViewModels.TLevels.AddTLevel.SaveDetails.Command command)
        {
            await _mediator.Send(command);

            return RedirectToAction(
                "Index",
                "AddVenue",
                new
                {
                    returnUrl = new Url(
                        Url.Action(
                            nameof(Details),
                            new { ffiid = _journeyInstance.InstanceId.UniqueKey }))
                        .WithProviderContext(_providerContext)
                })
                .WithProviderContext(_providerContext);
        }

        [HttpGet("check-publish")]
        [RequireJourneyInstance]
        public async Task<IActionResult> CheckAndPublish()
        {
            var query = new ViewModels.TLevels.AddTLevel.CheckAndPublish.Query();
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }

        [HttpPost("check-publish")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Publish()
        {
            var command = new ViewModels.TLevels.AddTLevel.CheckAndPublish.Command() { ProviderId = _providerContext.ProviderInfo.ProviderId };
            return await _mediator.SendAndMapResponse(
                command,
                response => response.Match<IActionResult>(
                    errors => this.ViewFromErrors(nameof(CheckAndPublish), errors),
                    success => RedirectToAction(nameof(Published))
                        .WithProviderContext(_providerContext)
                        .WithJourneyInstanceUniqueKey(_journeyInstance)));
        }

        [HttpGet("success")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Published()
        {
            var query = new ViewModels.TLevels.AddTLevel.Published.Query();

            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            string commonurl = "find-a-course/tdetails?tlevelId=";
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) + commonurl;

            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }
    }
}
