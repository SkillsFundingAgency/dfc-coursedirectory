using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.ManualAddress;
using Dfc.CourseDirectory.Web.ViewComponents.Shared;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ILogger<VenuesController> _logger;
        private readonly IPostCodeSearchService _postCodeSearchService;
        private readonly IVenueAddService _venueAddService;
        private readonly IVenueSearchSettings _venueSearchSettings;
        private readonly IVenueSearchService _venueSearchService;
        private readonly IVenueSearchHelper _venueSearchHelper;

        public VenuesController(
            ILogger<VenuesController> logger,
            IPostCodeSearchService postCodeSearchService,
            IVenueAddService venueAddService,
            IOptions<VenueSearchSettings> venueSearchSettings,
            IVenueSearchService venueSearchService,
            IVenueSearchHelper venueSearchHelper)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(postCodeSearchService, nameof(postCodeSearchService));
            Throw.IfNull(venueAddService, nameof(venueAddService));
            Throw.IfNull(venueSearchSettings, nameof(venueSearchSettings));
            Throw.IfNull(venueSearchService, nameof(venueSearchService));

            _logger = logger;
            _postCodeSearchService = postCodeSearchService;
            _venueAddService = venueAddService;
            _venueSearchSettings = venueSearchSettings.Value;
            _venueSearchService = venueSearchService;
            _venueSearchHelper = venueSearchHelper;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddVenue()
        {
            return View();
        }

        public IActionResult AddVenueManualAddress()
        {
            return View();
        }

        public async Task<IActionResult> VenueAddressSelectionConfirmation(VenueAddressSelectionConfirmationRequestModel requestModel)
        {
            var viewModel = new VenueAddressSelectionConfirmationViewModel();
            var criteria = new AddressSelectionCriteria(requestModel.PostcodeId);
            var searchResult = await _postCodeSearchService.RetrieveAsync(criteria);

            if (searchResult.IsSuccess && searchResult.HasValue)
            {
                viewModel.Address = new AddressModel
                {
                    Id = searchResult.Value.Id,
                    AddressLine1 = searchResult.Value.Line1,
                    AddressLine2 = searchResult.Value.Line2,
                    TownOrCity = searchResult.Value.City,
                    County = searchResult.Value.County,
                    Postcode = searchResult.Value.PostCode
                };
            }
            else
            {
                viewModel.Error = searchResult.Error;
            }

            viewModel.VenueName = requestModel.VenueName;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddVenueSelectionConfirmation(AddVenueSelectionConfirmationRequestModel requestModel)
        {
            VenueAdd venue = new VenueAdd(requestModel.AddressLine1, requestModel.AddressLine2, requestModel.TownOrCity, requestModel.VenueName, requestModel.County, requestModel.Postcode, "10028015");
            var addedVenue = await _venueAddService.AddAsync(venue);

            VenueSearchResultModel resultModel;

            VenueSearchRequestModel mod = new VenueSearchRequestModel();
            mod.SearchTerm = "10028015";
            mod.NewAddressId = addedVenue.Value.Id;

            var criteria = _venueSearchHelper.GetVenueSearchCriteria(mod);
            var result = await _venueSearchService.SearchAsync(criteria);

            VenueSearchResultItemModel newItem = new VenueSearchResultItemModel(requestModel.VenueName, requestModel.AddressLine1, requestModel.AddressLine2, requestModel.TownOrCity, requestModel.County, requestModel.Postcode);

            if (result.IsSuccess && result.HasValue)
            {
                var items = _venueSearchHelper.GetVenueSearchResultItemModels(result.Value.Value);
                resultModel = new VenueSearchResultModel(
                    mod.SearchTerm,
                    items, newItem);
            }
            else
            {
                resultModel = new VenueSearchResultModel(result.Error);
            }

            var viewModel = new VenueSearchResultsViewModel
            {
                Result = resultModel
            };

            return View("VenueSearchResults", viewModel);
        }

        [HttpPost]
        public IActionResult VenueAddressManualConfirmation(AddVenueSelectionConfirmationRequestModel model)
        {
            var viewModel = new VenueAddressSelectionConfirmationViewModel
            {
                VenueName = model.VenueName,
                Address = new AddressModel
                {
                    AddressLine1 = model.AddressLine1,
                    AddressLine2 = model.AddressLine2,
                    TownOrCity = model.TownOrCity,
                    County = model.County,
                    Postcode = model.Postcode
                }
            };

            return View(viewModel);
        }
    }
}