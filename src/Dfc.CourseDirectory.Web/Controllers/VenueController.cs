﻿using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.AddressSelectionConfirmation;
using Dfc.CourseDirectory.Web.ViewComponents.EditVenueName;
using Dfc.CourseDirectory.Web.ViewComponents.ManualAddress;
using Dfc.CourseDirectory.Web.ViewComponents.PostCodeSearchResult;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearch;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class VenueController : Controller
    {
        private readonly ILogger<VenueController> _logger;
        private readonly IPostCodeSearchService _postCodeSearchService;
        private readonly IVenueAddService _venueAddService;
        private readonly IVenueSearchSettings _venueSearchSettings;
        private readonly IVenueSearchService _venueSearchService;
        private readonly IVenueSearchHelper _venueSearchHelper;


        public VenueController(
            ILogger<VenueController> logger,
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

        public async Task<IActionResult> Index()
        {

            return View();
        }

        public IActionResult AddVenue()
        {
            //VenueAdd venue = new VenueAdd("Address1", "Address2", "town", "venuename", "county", "b71 4du");
            //var t = await _venueAddService.AddAsync(venue);
            return View();
        }

        public IActionResult PostCodeSearch()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmSelection()
        {
            //DEBUG


            PostCodeSearchResultModel model = new PostCodeSearchResultModel();
            model.VenueName = "test venue name";
            model.Id = "GB|RM|B|51879423";
            AddressSelectionCriteria criteria = new AddressSelectionCriteria(model.Id);

            var retrievedAddress = await _postCodeSearchService.RetrieveAsync(criteria);

            AddressSelectionConfirmationModel addressSelectionConfirmationModel =
                new AddressSelectionConfirmationModel
                {
                    VenueName = model.VenueName,
                    Id = model.Id,
                    PostCode = retrievedAddress.Value.PostCode,
                    Town = retrievedAddress.Value.City,
                    AddressLine1 = retrievedAddress.Value.Line1,
                    AddressLine2 = retrievedAddress.Value.Line2,
                    County = retrievedAddress.Value.County
                };

            return View(addressSelectionConfirmationModel);
        }



        public IActionResult AddVenueManual()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> ConfirmSelection(PostCodeSearchResultModel model)
        {

            AddressSelectionCriteria criteria  = new AddressSelectionCriteria(model.Id);

            var retrievedAddress = await _postCodeSearchService.RetrieveAsync(criteria);

            AddressSelectionConfirmationModel addressSelectionConfirmationModel =
                new AddressSelectionConfirmationModel
                {
                    VenueName = model.VenueName,
                    Id = model.Id,
                    PostCode = retrievedAddress.Value.PostCode,
                    Town = retrievedAddress.Value.City,
                    AddressLine1 = retrievedAddress.Value.Line1,
                    AddressLine2 = retrievedAddress.Value.Line2,
                    County = retrievedAddress.Value.County
                };

            return View(addressSelectionConfirmationModel);
        }
        
        [HttpPost]
        public async Task<IActionResult> AddVenueConfirmation(AddressSelectionConfirmationModel model)
        {
            //save it............

            VenueAdd venue = new VenueAdd(model.AddressLine1, model.AddressLine2, model.Town,model.VenueName, model.County,model.PostCode, "10028015");
            var addedVenue = await _venueAddService.AddAsync(venue);

            VenueSearchResultModel resultModel;
            VenueSearchRequestModel mod = new VenueSearchRequestModel();
            mod.SearchTerm = "10028015";
            mod.NewAddressId = addedVenue.Value.Id;

            var criteria = _venueSearchHelper.GetVenueSearchCriteria(mod);

            var result = await _venueSearchService.SearchAsync(criteria);

            VenueSearchResultItemModel newItem = new VenueSearchResultItemModel(model.VenueName,model.AddressLine1,model.AddressLine2,model.Town,model.County,model.PostCode);

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

            return View("index", model);
        }
        [HttpGet]
        public IActionResult EditVenueName(AddressSelectionConfirmationModel model)
        {
            AddressSelectionConfirmationModel override_model = new AddressSelectionConfirmationModel
            {
                Id = "",
                VenueName = "My House",
                AddressLine1 = "222",
                AddressLine2 = "eee",
                Town = "ff",
                County = "dd",
                PostCode = "dggg"
            };

            EditVenueNameModel editModel = new EditVenueNameModel
            {
                Id = override_model.Id,
                VenueName = override_model.VenueName,
                AddressLine1 = override_model.AddressLine1,
                AddressLine2 = override_model.AddressLine2,
                Town = override_model.Town,
                County = override_model.County,
                PostCode = override_model.PostCode
            };
            return View(editModel);
        }

        [HttpPost]
        public IActionResult EditVenueName(EditVenueNameModel model)
        {
            return View();
        }

        [HttpPost]
        public IActionResult ConfirmManualSelection(ManualAddressModel model)
        {
            var addressSelectionConfirmationModel = new AddressSelectionConfirmationModel
            {
                VenueName = model.VenueName == null ? model.VenueName : model.VenueName.Trim(),
                Id = model.Id,
                PostCode = model.Postcode,
                Town = model.TownCity == null ? model.TownCity : model.TownCity.Trim() ,
                AddressLine1 = model.AddressLine1 == null ? model.AddressLine1 : model.AddressLine1.Trim(),
                AddressLine2 = model.AddressLine2 == null ? model.AddressLine2 : model.AddressLine2.Trim(),
                County = model.County == null ? model.County : model.County.Trim() 
            };

            return View(addressSelectionConfirmationModel);
        }

        public async Task<IActionResult> AddAddressManually()
        {
            return View();
        }
    }
}