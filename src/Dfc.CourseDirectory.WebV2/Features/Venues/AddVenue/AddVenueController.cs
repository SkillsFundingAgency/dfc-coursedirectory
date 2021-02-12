using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Flurl;
using FormFlow;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.AddVenue
{
    [Route("venues/add")]
    [JourneyMetadata(
        journeyName: "AddVenue",
        stateType: typeof(AddVenueJourneyModel),
        appendUniqueKey: true,
        requestDataKeys: "providerId?")]
    [RequireProviderContext]
    public class AddVenueController : Controller
    {
        private const string ReturnUrlJourneyInstancePropertyKey = "returnUrl";

        private readonly IMediator _mediator;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;
        private readonly IProviderContextProvider _providerContextProvider;
        private JourneyInstance<AddVenueJourneyModel> _journeyInstance;

        public AddVenueController(
            IMediator mediator,
            JourneyInstanceProvider journeyInstanceProvider,
            IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _journeyInstanceProvider = journeyInstanceProvider;
            _providerContextProvider = providerContextProvider;
            _journeyInstance = _journeyInstanceProvider.GetInstance<AddVenueJourneyModel>();
        }

        [HttpGet("")]
        public IActionResult Index([FromQuery] string postcode, [FromQuery] string returnUrl)
        {
            _journeyInstance = _journeyInstanceProvider.GetOrCreateInstance(
                () => new AddVenueJourneyModel(),
                new PropertiesBuilder()
                    .Add(ReturnUrlJourneyInstancePropertyKey, returnUrl)
                    .Build());

            if (!_journeyInstanceProvider.IsCurrentInstance(_journeyInstance))
            {
                return RedirectToAction()
                    .WithProviderContext(_providerContextProvider.GetProviderContext())
                    .WithJourneyInstanceUniqueKey(_journeyInstance);
            }

            return View(
                nameof(PostcodeSearch),
                new PostcodeSearch.SearchCommand()
                {
                    Postcode = postcode
                });
        }

        [HttpPost("cancel")]
        [RequireJourneyInstance]
        public IActionResult Cancel()
        {
            _journeyInstance.Delete();

            if (_journeyInstance.Properties.TryGetValue(ReturnUrlJourneyInstancePropertyKey, out var returnUrlObj) &&
                returnUrlObj is string returnUrl &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(ViewVenues.ViewVenuesController.ViewVenues), "ViewVenues")
                .WithProviderContext(_providerContextProvider.GetProviderContext());
        }

        [HttpGet("postcode-search")]
        [RequireJourneyInstance]
        public async Task<IActionResult> PostcodeSearch(PostcodeSearch.SearchCommand command) =>
            await _mediator.SendAndMapResponse(
                command,
                result => result.Match(
                    errors => this.ViewFromErrors(errors),
                    vm => View("PostcodeSearchResults", vm)));

        [HttpPost("select-postcode")]
        [RequireJourneyInstance]
        public async Task<IActionResult> SelectPostcode(PostcodeSearch.SelectCommand command, [FromQuery] string postcode)
        {
            command.Postcode = postcode;

            return await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors("PostcodeSearchResults", errors),
                    success => RedirectToAction(nameof(Details))
                        .WithProviderContext(_providerContextProvider.GetProviderContext())
                        .WithJourneyInstanceUniqueKey(_journeyInstance)));
        }

        [HttpGet("address")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Address([FromQuery] string postcode, [FromQuery] bool? fromPublishPage) =>
            await _mediator.SendAndMapResponse(
                new Address.Query(),
                vm =>
                {
                    vm.Postcode ??= Postcode.TryParse(postcode, out var pc) ? pc : postcode;
                    return View(vm).WithViewData("FromPublishPage", fromPublishPage);
                });

        [HttpPost("address")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Address(Address.Command command, [FromQuery] bool? fromPublishPage) =>
            await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(fromPublishPage == true ? nameof(CheckAndPublish) : nameof(Details))
                        .WithProviderContext(_providerContextProvider.GetProviderContext())
                        .WithJourneyInstanceUniqueKey(_journeyInstance)));

        [HttpGet("details")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Details([FromQuery] bool? fromPublishPage) =>
            await _mediator.SendAndMapResponse(
                new Details.Query(),
                vm => View(vm).WithViewData("FromPublishPage", fromPublishPage));

        [HttpPost("details")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Details(Details.Command command)
        {
            command.ProviderId = _providerContextProvider.GetProviderContext().ProviderInfo.ProviderId;

            return await _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(CheckAndPublish))
                        .WithProviderContext(_providerContextProvider.GetProviderContext())
                        .WithJourneyInstanceUniqueKey(_journeyInstance)));
        }

        [HttpGet("check-publish")]
        [RequireJourneyInstance]
        public async Task<IActionResult> CheckAndPublish() => await _mediator.SendAndMapResponse(
            new CheckAndPublish.Query(),
            vm => View(vm));

        [HttpPost("publish")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Publish(CheckAndPublish.Command command) =>
            await _mediator.SendAndMapResponse(
                command,
                success =>
                {
                    if (_journeyInstance.Properties.TryGetValue(ReturnUrlJourneyInstancePropertyKey, out var returnUrlObj) &&
                        returnUrlObj is string returnUrl &&
                        Url.IsLocalUrl(returnUrl))
                    {
                        var venueId = _journeyInstance.State.VenueId.Value;
                        return Redirect(new Url(returnUrl).SetQueryParam("venueId", venueId));
                    }

                    return (IActionResult)RedirectToAction(nameof(Published))
                        .WithProviderContext(_providerContextProvider.GetProviderContext())
                        .WithJourneyInstanceUniqueKey(_journeyInstance);
                });

        [HttpGet("success")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Published()
        {
            var query = new Published.Query();
            return await _mediator.SendAndMapResponse(query, vm => View(vm));
        }
    }
}
