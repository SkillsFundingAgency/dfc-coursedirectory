﻿using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Onspd;
using Dfc.CourseDirectory.Services.Models.Venues;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Extensions;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.EditVenueAddress;
using Dfc.CourseDirectory.Web.ViewComponents.EditVenueName;
using Dfc.CourseDirectory.Web.ViewComponents.Shared;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.WebV2;
using Dfc.CourseDirectory.WebV2.AddressSearch;
using Flurl;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Dfc.CourseDirectory.Services.Models.Venues.VenueStatus;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class VenuesController : Controller
    {
        private readonly IAddressSearchService _addressSearchService;
        private readonly IVenueSearchHelper _venueSearchHelper;
        private readonly IVenueService _venueService;
        private readonly ISearchClient<Core.Search.Models.Onspd> _searchClient;
        private readonly SqlDataSync _sqlDataSync;

        private ISession Session => HttpContext.Session;

        public VenuesController(
            IAddressSearchService addressSearchService,
            IVenueSearchHelper venueSearchHelper,
            IVenueService venueService,
            ISearchClient<Core.Search.Models.Onspd> searchClient,
            SqlDataSync sqlDataSync)
        {
            _addressSearchService = addressSearchService ?? throw new ArgumentNullException(nameof(addressSearchService));
            _venueSearchHelper = venueSearchHelper ?? throw new ArgumentNullException(nameof(venueSearchHelper));
            _venueService = venueService ?? throw new ArgumentNullException(nameof(venueService));
            _searchClient = searchClient ?? throw new ArgumentNullException(nameof(searchClient));
            _sqlDataSync = sqlDataSync;
        }

        [Authorize]
        public IActionResult AddVenue([FromQuery] string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                Session.SetString("ADDNEWVENUERETURNURL", returnUrl);
            }

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
                
                if (getVenueByIdResult.IsSuccess)
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
        public IActionResult EditVenue(string Id) => RedirectToAction("Details", "EditVenue", new { venueId = Id });
        
        [Authorize]
        public async Task<IActionResult> VenueAddressSelectionConfirmation(VenueAddressSelectionConfirmationRequestModel requestModel, string VenueName)
        {
            var viewModel = new VenueAddressSelectionConfirmationViewModel();

            if (!string.IsNullOrEmpty(requestModel.PostcodeId))
            {
                var searchResult = await _addressSearchService.GetById(requestModel.PostcodeId);

                if (searchResult != null)
                {
                    viewModel.Address = new AddressModel
                    {
                        //Id = searchResult.Value.Id,
                        AddressLine1 = searchResult.Line1,
                        AddressLine2 = searchResult.Line2,
                        TownOrCity = searchResult.PostTown,
                        County = searchResult.County,
                        Postcode = searchResult.Postcode,
                        Country = searchResult.CountryName
                    };
                    viewModel.Id = requestModel.Id;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(requestModel.Id))
                {
                    var criteria = new GetVenueByIdCriteria(requestModel.Id);
                    var getVenueByIdResult = await _venueService.GetVenueByIdAsync(criteria);

                    if (getVenueByIdResult.IsSuccess)
                    {
                        var onspd = await GetOnsPostcodeData(getVenueByIdResult.Value.PostCode);

                        viewModel.Address = new AddressModel
                        {
                            //Id = getVenueByIdResult.Value.ID,
                            AddressLine1 = getVenueByIdResult.Value.Address1,
                            AddressLine2 = getVenueByIdResult.Value.Address2,
                            TownOrCity = getVenueByIdResult.Value.Town,
                            County = getVenueByIdResult.Value.County,
                            Postcode = getVenueByIdResult.Value.PostCode,
                            Country = onspd?.Country
                        };
                    }

                    viewModel.Id = requestModel.Id;
                }
            }

            if (!string.IsNullOrEmpty(requestModel.PostcodeId))
            {
                viewModel.PostCodeId = requestModel.PostcodeId;
            }

            viewModel.VenueName = requestModel.VenueName;

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddVenueSelectionConfirmation(AddVenueSelectionConfirmationRequestModel requestModel)
        {
            var UKPRN = Session.GetInt32("UKPRN");

            bool updated = false;
            string venueID = string.Empty;
            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }


            var onspd = await GetOnsPostcodeData(requestModel.Postcode);
            var latitude = onspd.lat;
            var longitude = onspd.@long;

            if (requestModel.Id != null)
            {
                Venue venue = new Venue(
                    requestModel.Id,
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
                    Live,
                    "TestUser",
                    DateTime.Now
                );

                var updatedVenue = await _venueService.UpdateAsync(venue);
                await _sqlDataSync.SyncVenue(venue);
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
                    Live,
                    "TestUser",
                    DateTime.Now,
                    DateTime.Now
                    );

                var addedVenue = await _venueService.AddAsync(venue);
                venueID = addedVenue.Value.ID;
                venue.ID = venueID;
                await _sqlDataSync.SyncVenue(venue);
            }
            //Since we are updating or adding lets pass the model to the GetVenues method
            VenueSearchResultItemModel newItem = new VenueSearchResultItemModel(HtmlEncoder.Default.Encode(requestModel.VenueName), requestModel.AddressLine1, requestModel.AddressLine2, requestModel.TownOrCity, requestModel.County, requestModel.Postcode, venueID);

            var addedVenueModel = new AddedVenueModel
            {
                VenueName = HtmlEncoder.Default.Encode(requestModel.VenueName),
                AddressLine1 = requestModel.AddressLine1,
                AddressLine2 = requestModel.AddressLine2,
                County = requestModel.County,
                Id = venueID,
                PostCode = requestModel.Postcode,
                Town = requestModel.TownOrCity
            };


            var returnUrl = Session.GetString("ADDNEWVENUERETURNURL");
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(new Url(returnUrl).SetQueryParam("venueId", venueID));
            }


            string option = Session.GetString("Option") ?? string.Empty;

            if (option.ToUpper() == "ADDNEWVENUEFOREDIT")
            {
                Session.SetObject("NewAddedVenue", addedVenueModel);
                return RedirectToAction("Reload", "EditCourseRun");
            }

            if (option.ToUpper() == "ADDNEWVENUEFORCOPY")
            {
                Session.SetObject("NewAddedVenue", addedVenueModel);
                return RedirectToAction("Reload", "CopyCourseRun");
            }

            if (option.ToUpper() == "ADDNEWVENUE")
            {
                Session.SetObject("NewAddedVenue", addedVenueModel);
                return RedirectToAction("SummaryToAddCourseRun", "AddCourse");
            }
            if(option.ToUpper() == "ADDNEWVENUEFORAPPRENTICESHIPS")
            {
                Session.SetObject("NewAddedVenue", addedVenueModel);
                return RedirectToAction("DeliveryOptions", "Apprenticeships", new
                {
                    message = "",
                    mode = "Add"                    
                });
            }
            if (option.ToUpper() == "ADDNEWVENUEFORAPPRENTICESHIPSCOMBINED")
            {
                Session.SetObject("NewAddedVenue", addedVenueModel);
                return RedirectToAction("DeliveryOptionsCombined", "Apprenticeships", new
                {
                    message = "",
                    mode = "Add"
                });
            }

            return View("VenueSearchResults", await GetVenues(UKPRN.Value, newItem, updated));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> VenueAddressManualConfirmation(AddVenueSelectionConfirmationRequestModel model)
        {
            var onspd = await GetOnsPostcodeData(model.Postcode);

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
                    Postcode = model.Postcode,
                    Country = onspd?.Country
                }
            };

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditVenueName(EditVenueRequestModel requestModel)
        {
            AddressModel addressModel = null;

            if (!string.IsNullOrEmpty(requestModel.Id))
            {
                GetVenueByIdCriteria criteria = new GetVenueByIdCriteria(requestModel.Id);

                var getVenueByIdResult = await _venueService.GetVenueByIdAsync(criteria);
                if (getVenueByIdResult.IsSuccess)
                {
                    addressModel = new AddressModel
                    {
                        //Id = getVenueByIdResult.Value.ID,
                        AddressLine1 = getVenueByIdResult.Value.Address1,
                        AddressLine2 = getVenueByIdResult.Value.Address2,
                        TownOrCity = getVenueByIdResult.Value.Town,
                        County = getVenueByIdResult.Value.County,
                        Postcode = getVenueByIdResult.Value.PostCode
                    };
                }

            }
            else
            {
                addressModel = requestModel.Address;
            }

            EditVenueNameModel model = new EditVenueNameModel
            {
                VenueName = requestModel.VenueName,
                PostcodeId = requestModel.PostcodeId,
                Address = addressModel,
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
                PostcodeId = requestModel.PostcodeId,
                Address = requestModel.Address
            };
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> CheckForVenue(string VenueName)
        {
            int? sUKPRN = Session.GetInt32("UKPRN");
            int UKPRN;
            if (!sUKPRN.HasValue)
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            else
                UKPRN = sUKPRN ?? 0;

            var result = await _venueService.GetVenuesByPRNAndNameAsync(new GetVenuesByPRNAndNameCriteria(UKPRN.ToString(), VenueName));

            if (result.IsSuccess && result.Value.Value.Any(x => x.Status == Live))// && result.Value.Value.FirstOrDefault()?.Status == VenueStatus.Live)
                return Ok(true);
            return Ok(false);
        }

        [Authorize]
        [HttpGet]
        public IActionResult LandingOptions()
        {
            return View("../Venues/LandingOptions/Index", new LocationsLandingViewModel());
        }

        [HttpPost]
        public IActionResult LandingOptions(LocationsLandingViewModel model)
        {
            switch (model.LocationsLandingOptions)
            {
                case LocationsLandingOptions.Add:
                    return RedirectToAction("AddVenue", "Venues");
                case LocationsLandingOptions.Manage:
                    return RedirectToAction("ViewVenues", "ViewVenues");
                default:
                    return RedirectToAction("LandingOptions", "Venues");
            }

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

            if (result.IsSuccess)
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

            // The v2 EditVenue journey sets a TempData entry after venue has been updated
            if (TempData.ContainsKey(TempDataKeys.UpdatedVenueId))
            {
                model = new VenueSearchResultModel(
                    model.SearchTerm,
                    model.Items,
                    newItem: model.Items.Single(v => v.Id == TempData[TempDataKeys.UpdatedVenueId].ToString()),
                    updated: true);
            }

            var viewModel = new VenueSearchResultsViewModel
            {
                Result = model
            };
            return viewModel;
        }

        private async Task<VenueSearchResultsViewModel> GetVenues(VenueSearchResultItemModel deletedVenue)
        {
            var UKPRN = Session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return null;
            }
            VenueSearchRequestModel requestModel = new VenueSearchRequestModel
            {
                SearchTerm = UKPRN.ToString()
            };

            VenueSearchResultModel model;
            var criteria = _venueSearchHelper.GetVenueSearchCriteria(requestModel);
            var result = await _venueService.SearchAsync(criteria);

            if (result.IsSuccess)
            {
                var items = _venueSearchHelper.GetVenueSearchResultItemModels(result.Value.Value);
                model = new VenueSearchResultModel(
                    items, deletedVenue);
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

        private async Task<Onspd> GetOnsPostcodeData(string postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode))
            {
                return new Onspd();
            }

            var searchResult = await _searchClient.Search(new OnspdSearchQuery
            {
                Postcode = postcode
            });

            return searchResult.Items.Count > 0
                ? searchResult.Items.Single().Adapt<Onspd>()
                : new Onspd();
        }
    }
}
