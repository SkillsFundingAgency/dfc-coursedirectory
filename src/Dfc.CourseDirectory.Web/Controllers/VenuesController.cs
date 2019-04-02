﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Onspd;
using Dfc.CourseDirectory.Models.Models.Venues;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces.OnspdService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.OnspdService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.EditVenueAddress;
using Dfc.CourseDirectory.Web.ViewComponents.EditVenueName;
using Dfc.CourseDirectory.Web.ViewComponents.ManualAddress;
using Dfc.CourseDirectory.Web.ViewComponents.Shared;
using Dfc.CourseDirectory.Web.ViewComponents.VenueName;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ILogger<VenuesController> _logger;
        private readonly IPostCodeSearchService _postCodeSearchService;
        private readonly IVenueServiceSettings _venueServiceSettings;
        private readonly IVenueSearchHelper _venueSearchHelper;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IVenueService _venueService;
        private readonly IOnspdSearchHelper _onspdSearchHelper;
        private ISession _session => _contextAccessor.HttpContext.Session;

        
        public VenuesController(
            ILogger<VenuesController> logger,
            IPostCodeSearchService postCodeSearchService,
            IOptions<VenueServiceSettings> venueSearchSettings,
            IVenueSearchHelper venueSearchHelper,
            IHttpContextAccessor contextAccessor,
            IVenueService venueService,
            IOnspdSearchHelper onspdSearchHelper)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(postCodeSearchService, nameof(postCodeSearchService));
            Throw.IfNull(venueSearchSettings, nameof(venueSearchSettings));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(venueService, nameof(venueService));

            _logger = logger;
            _postCodeSearchService = postCodeSearchService;
            _venueServiceSettings = venueSearchSettings.Value;
            _venueSearchHelper = venueSearchHelper;
            _contextAccessor = contextAccessor;
            _venueService = venueService;
            _onspdSearchHelper = onspdSearchHelper;
        }
        /// <summary>
        /// Need to return a VenueSearchResultModel within the VenueSearchResultModel
        /// </summary>
        /// <returns></returns>
        /// 
        [Authorize]
        public async Task<IActionResult> Index()
        {
            _session.SetString("Option", "Venues");
            var providerUKPRN = User.Claims.SingleOrDefault(x => x.Type == "UKPRN");
            if (providerUKPRN != null)
            {
                _session.SetInt32("UKPRN", Int32.Parse(providerUKPRN.Value));
            }

            int UKPRN = 0;
            if (_session.GetInt32("UKPRN").HasValue)
            {
                UKPRN = _session.GetInt32("UKPRN").Value;
                return View(await GetVenues(UKPRN));
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

           
         
        }
        private async Task<VenueSearchResultsViewModel> GetVenues(int ukprn)
        {
            return await GetVenues(ukprn, null, false);
        }
        private async Task<VenueSearchResultsViewModel> GetVenues(int ukprn, VenueSearchResultItemModel newVenue, bool updated)
        {
            VenueSearchRequestModel requestModel = new VenueSearchRequestModel
            {
                SearchTerm = ukprn.ToString()
            };
            if (null != newVenue) requestModel.NewAddressId = newVenue.Id;


            VenueSearchResultModel model;
            var criteria = _venueSearchHelper.GetVenueSearchCriteria(requestModel);
            var result = await _venueService.SearchAsync(criteria);
            if (result.IsSuccess && result.HasValue)
            {
                var items = _venueSearchHelper.GetVenueSearchResultItemModels(result.Value.Value);
                model = new VenueSearchResultModel(
                    requestModel.SearchTerm,
                    items, newVenue, updated);
            }
            else
            {
                model = new VenueSearchResultModel(result.Error);
            }

            var viewModel = new VenueSearchResultsViewModel
            {
                Result = model
            };
            return viewModel;
        }
        [Authorize]
        public IActionResult AddVenue()
        {
            //_session.SetString("IsEdit", "false");
            return View();
        }
        [Authorize]
        public async Task<IActionResult> AddVenueManualAddress(string Id)
        {
            var viewModel = new VenueAddressSelectionConfirmationViewModel();

            if (Id != null)
            {
                GetVenueByIdCriteria criteria = new GetVenueByIdCriteria(Id);

                var getVenueByIdResult = await _venueService.GetVenueByIdAsync(criteria);
                if (getVenueByIdResult.IsSuccess && getVenueByIdResult.HasValue)
                {
                    viewModel.Id = Id;
                    viewModel.VenueName = getVenueByIdResult.Value.VenueName;
                    viewModel.Address = new AddressModel
                    {
                        AddressLine1 = getVenueByIdResult.Value.Address1,
                        AddressLine2 = getVenueByIdResult.Value.Address2,
                        TownOrCity = getVenueByIdResult.Value.Town,
                        County = getVenueByIdResult.Value.County,
                        Postcode = getVenueByIdResult.Value.PostCode
                    };
                }
                else
                {
                    viewModel.Error = getVenueByIdResult.Error;
                }

            }

            viewModel.Id = Id;

            return View(viewModel);
        }
        [Authorize]
        public async Task<IActionResult> EditVenue(string Id)
        {
            var viewModel = new VenueAddressSelectionConfirmationViewModel();

            GetVenueByIdCriteria criteria = new GetVenueByIdCriteria(Id);

            var getVenueByIdResult = await _venueService.GetVenueByIdAsync(criteria);
            if (getVenueByIdResult.IsSuccess && getVenueByIdResult.HasValue)
            {
                viewModel.Id = Id;
                viewModel.VenueName = getVenueByIdResult.Value.VenueName;
                viewModel.Address = new AddressModel
                {
                    //Id = getVenueByIdResult.Value.ID,
                    AddressLine1 = getVenueByIdResult.Value.Address1,
                    AddressLine2 = getVenueByIdResult.Value.Address2,
                    TownOrCity = getVenueByIdResult.Value.Town,
                    County = getVenueByIdResult.Value.County,
                    Postcode = getVenueByIdResult.Value.PostCode
                };
            }
            else
            {
                viewModel.Error = getVenueByIdResult.Error;
            }

            viewModel.Id = Id;

            return View("VenueAddressSelectionConfirmation", viewModel);
        }
        [Authorize]
        public async Task<IActionResult> VenueAddressSelectionConfirmation(VenueAddressSelectionConfirmationRequestModel requestModel)
        {
            var viewModel = new VenueAddressSelectionConfirmationViewModel();

            if (!string.IsNullOrEmpty(requestModel.PostcodeId))
            {
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
                    viewModel.Id = requestModel.Id;
                }
                else
                {
                    viewModel.Error = searchResult.Error;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(requestModel.Id))
                {
                    var criteria = new GetVenueByIdCriteria(requestModel.Id);
                    var getVenueByIdResult = await _venueService.GetVenueByIdAsync(criteria);

                    if (getVenueByIdResult.IsSuccess && getVenueByIdResult.HasValue)
                    {
                        viewModel.Address = new AddressModel
                        {
                            //Id = getVenueByIdResult.Value.ID,
                            AddressLine1 = getVenueByIdResult.Value.Address1,
                            AddressLine2 = getVenueByIdResult.Value.Address2,
                            TownOrCity = getVenueByIdResult.Value.Town,
                            County = getVenueByIdResult.Value.County,
                            Postcode = getVenueByIdResult.Value.PostCode
                        };
                    }

                    viewModel.Id = requestModel.Id;
                }
            }

            viewModel.VenueName = requestModel.VenueName;

            return View(viewModel);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddVenueSelectionConfirmation(AddVenueSelectionConfirmationRequestModel requestModel)
        {
            var UKPRN = _session.GetInt32("UKPRN");

            bool updated = false;
            string venueID = string.Empty;
            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }
            

            var onspd = _onspdSearchHelper.GetOnsPostcodeData(requestModel.Postcode);
            var latitude = onspd.lat;
            var longitude = onspd.@long;

            if (requestModel.Id != null)
            {
                Venue venue = new Venue(
                    requestModel.Id,
                    UKPRN.Value,
                    requestModel.VenueName,
                    requestModel.AddressLine1,
                    (null != requestModel.AddressLine2 ? requestModel.AddressLine2 : string.Empty) ,
                    null,
                    requestModel.TownOrCity,
                    (null != requestModel.County ? requestModel.County : string.Empty), 
                    requestModel.Postcode,
                    latitude,
                    longitude,
                    VenueStatus.Live,
                    "TestUser",
                    DateTime.Now
                );

                var updatedVenue = await _venueService.UpdateAsync(venue);
                updated = true;
                venueID = updatedVenue.Value.ID;
              
            }
            else
            {
                Venue venue = new Venue(
                    null,
                     UKPRN.Value,
                    requestModel.VenueName,
                    requestModel.AddressLine1,
                    (null != requestModel.AddressLine2 ? requestModel.AddressLine2 : string.Empty),
                    null,
                    requestModel.TownOrCity,
                    (null != requestModel.County ? requestModel.County : string.Empty),
                    requestModel.Postcode,
                    latitude,
                    longitude,
                    VenueStatus.Live,
                    "TestUser",
                    DateTime.Now,
                    DateTime.Now
                    );

                var addedVenue = await _venueService.AddAsync(venue);
                venueID = addedVenue.Value.ID;
            }
            //Since we are updating or adding lets pass the model to the GetVenues method
            VenueSearchResultItemModel newItem = new VenueSearchResultItemModel(HttpUtility.HtmlEncode(requestModel.VenueName), requestModel.AddressLine1, requestModel.AddressLine2, requestModel.TownOrCity, requestModel.County, requestModel.Postcode, venueID);

            return View("VenueSearchResults", await GetVenues(UKPRN.Value, newItem, updated));
        }
        [Authorize]
        [HttpPost]
        public IActionResult VenueAddressManualConfirmation(AddVenueSelectionConfirmationRequestModel model)
        {
            var viewModel = new VenueAddressSelectionConfirmationViewModel
            {
                Id = model.Id,
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
        [Authorize]
        [HttpPost]
        public IActionResult EditVenueName(EditVenueRequestModel requestModel)
        {
            EditVenueNameModel model = new EditVenueNameModel
            {
                VenueName = requestModel.VenueName,
                PostcodeId = requestModel.Address.Id,
                Address = requestModel.Address,
                Id = requestModel.Id
            };
            return View(model);
        }
        [Authorize]
        [HttpPost]
        public IActionResult EditVenueAddress(EditVenueRequestModel requestModel)
        {
            EditVenueAddressModel model = new EditVenueAddressModel
            {
                Id = requestModel.Id,
                VenueName = requestModel.VenueName,
                PostcodeId = requestModel.Address.Id,
                Address = requestModel.Address
            };
            return View(model);
        }


    }
}