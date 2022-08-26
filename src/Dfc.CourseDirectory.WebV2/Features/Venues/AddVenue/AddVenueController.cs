using System.Linq;
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

        public AddVenueController(
            IMediator mediator,
            JourneyInstanceProvider journeyInstanceProvider,
            IProviderContextProvider providerContextProvider)
        {
            _mediator = mediator;
            _journeyInstanceProvider = journeyInstanceProvider;
            _providerContextProvider = providerContextProvider;
        }

        [HttpGet("")]
        public IActionResult Index([FromQuery] string postcode, [FromQuery] string returnUrl)
        {
            var journeyInstance = _journeyInstanceProvider.GetOrCreateInstance(
                () => new AddVenueJourneyModel(),
                new PropertiesBuilder()
                    .Add(ReturnUrlJourneyInstancePropertyKey, returnUrl)
                    .Build());

            if (!_journeyInstanceProvider.IsCurrentInstance(journeyInstance))
            {
                return RedirectToAction()
                    .WithProviderContext(_providerContextProvider.GetProviderContext())
                    .WithJourneyInstanceUniqueKey(journeyInstance);
            }

            return View(
                nameof(PostcodeSearch),
                new PostcodeSearch.SearchCommand()
                {
                    Postcode = postcode
                });
        }

        [HttpGet("cancel")]
        [RequireJourneyInstance]
        public IActionResult Cancel()
        {
            var journeyInstance = _journeyInstanceProvider.GetInstance<AddVenueJourneyModel>();
            journeyInstance.Delete();

            if (journeyInstance.Properties.TryGetValue(ReturnUrlJourneyInstancePropertyKey, out var returnUrlObj)
                && returnUrlObj is string returnUrl
                && Url.IsLocalUrl(returnUrl))
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
        public Task<IActionResult> SelectPostcode(PostcodeSearch.SelectCommand command)
        {
            var journeyInstance = _journeyInstanceProvider.GetInstance<AddVenueJourneyModel>();

            return _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors("PostcodeSearchResults", errors),
                    success => RedirectToAction(nameof(Details))
                        .WithProviderContext(_providerContextProvider.GetProviderContext())
                        .WithJourneyInstanceUniqueKey(journeyInstance)));
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
        public Task<IActionResult> Address(Address.Command command, [FromQuery] bool? fromPublishPage)
        {
            var journeyInstance = _journeyInstanceProvider.GetInstance<AddVenueJourneyModel>();

            return _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(fromPublishPage == true ? nameof(CheckAndPublish) : nameof(Details))
                        .WithProviderContext(_providerContextProvider.GetProviderContext())
                        .WithJourneyInstanceUniqueKey(journeyInstance)));
        }

        [HttpGet("details")]
        [RequireJourneyInstance]
        public async Task<IActionResult> Details([FromQuery] bool? fromPublishPage) =>
            await _mediator.SendAndMapResponse(
                new Details.Query(),
                vm => View(vm).WithViewData("FromPublishPage", fromPublishPage));

        [HttpPost("details")]
        [RequireJourneyInstance]
        public Task<IActionResult> Details(Details.Command command)
        {
            var journeyInstance = _journeyInstanceProvider.GetInstance<AddVenueJourneyModel>();

            command.ProviderId = _providerContextProvider.GetProviderContext().ProviderInfo.ProviderId;

            return _mediator.SendAndMapResponse(
                command,
                result => result.Match<IActionResult>(
                    errors => this.ViewFromErrors(errors),
                    success => RedirectToAction(nameof(CheckAndPublish))
                        .WithProviderContext(_providerContextProvider.GetProviderContext())
                        .WithJourneyInstanceUniqueKey(journeyInstance)));
        }

        [HttpGet("check-publish")]
        [RequireJourneyInstance]
        public async Task<IActionResult> CheckAndPublish() => await _mediator.SendAndMapResponse(
            new CheckAndPublish.Query(),
            vm => View(vm));

        [HttpPost("publish")]
        [RequireJourneyInstance]
        public Task<IActionResult> Publish(CheckAndPublish.Command command) =>
            _mediator.SendAndMapResponse(
                command,
                success =>
                {
                    var journeyInstance = _journeyInstanceProvider.GetInstance<AddVenueJourneyModel>();

                    // LEGACY: Some legacy journeys display a notification when a venue has been added.
                    // Stash the venue's address in TempData so it can be read by those legacy views.
                    TempData[TempDataKeys.AddedVenueDescription] =
                        string.Join(
                            ", ",
                            new[]
                            {
                                journeyInstance.State.Name,
                                journeyInstance.State.AddressLine1,
                                journeyInstance.State.AddressLine2,
                                journeyInstance.State.Town,
                                journeyInstance.State.County,
                                journeyInstance.State.Postcode
                            }
                            .Where(l => !string.IsNullOrWhiteSpace(l)));

                    if (journeyInstance.Properties.TryGetValue(ReturnUrlJourneyInstancePropertyKey, out var returnUrlObj)
                        && returnUrlObj is string returnUrl
                        && Url.IsLocalUrl(returnUrl))
                    {
                        var venueId = journeyInstance.State.VenueId.Value;
                        return Redirect(new Url(returnUrl).SetQueryParam("venueId", venueId));
                    }

                    return (IActionResult)RedirectToAction(nameof(Published))
                        .WithProviderContext(_providerContextProvider.GetProviderContext())
                        .WithJourneyInstanceUniqueKey(journeyInstance);
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
