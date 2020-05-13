using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interfaces.Venues;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Venues;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Extensions;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.EditVenueAddress;
using Dfc.CourseDirectory.Web.ViewComponents.EditVenueName;
using Dfc.CourseDirectory.Web.ViewComponents.Shared;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.WebV2.LoqateAddressSearch;
using Flurl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Dfc.CourseDirectory.Models.Models.Venues.VenueStatus;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ILogger<VenuesController> _logger;
        private readonly IAddressSearchService _addressSearchService;
        private readonly IVenueServiceSettings _venueServiceSettings;
        private readonly IVenueSearchHelper _venueSearchHelper;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IVenueService _venueService;
        private readonly IOnspdSearchHelper _onspdSearchHelper;
        private readonly ICourseService _courseService;
        private readonly IApprenticeshipService _apprenticeshipService;

        private ISession _session => _contextAccessor.HttpContext.Session;



        public VenuesController(
            ILogger<VenuesController> logger,
            IAddressSearchService addressSearchService,
            IOptions<VenueServiceSettings> venueSearchSettings,
            IVenueSearchHelper venueSearchHelper,
            IHttpContextAccessor contextAccessor,
            IVenueService venueService,
            IOnspdSearchHelper onspdSearchHelper,
            ICourseService courseService, 
            IApprenticeshipService apprenticeshipService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(addressSearchService, nameof(addressSearchService));
            Throw.IfNull(venueSearchSettings, nameof(venueSearchSettings));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(venueService, nameof(venueService));
            Throw.IfNull(courseService, nameof(courseService));

            _logger = logger;
            _addressSearchService = addressSearchService;
            _venueServiceSettings = venueSearchSettings.Value;
            _venueSearchHelper = venueSearchHelper;
            _contextAccessor = contextAccessor;
            _venueService = venueService;
            _onspdSearchHelper = onspdSearchHelper;
            _courseService = courseService;
            _apprenticeshipService = apprenticeshipService;
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
            Claim providerUKPRN = User.Claims.Where(x => x.Type == "UKPRN").SingleOrDefault();
            if (!String.IsNullOrEmpty(providerUKPRN?.Value))
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

        private async Task<VenueSearchResultsViewModel> GetVenues(VenueSearchResultItemModel deletedVenue)
        {
            var UKPRN = _session.GetInt32("UKPRN");

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
            if (result.IsSuccess && result.HasValue)
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

        [Authorize]
        public IActionResult AddVenue([FromQuery] string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                _contextAccessor.HttpContext.Session.SetString("ADDNEWVENUERETURNURL", returnUrl);
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
            }
            //Since we are updating or adding lets pass the model to the GetVenues method
            VenueSearchResultItemModel newItem = new VenueSearchResultItemModel(HttpUtility.HtmlEncode(requestModel.VenueName), requestModel.AddressLine1, requestModel.AddressLine2, requestModel.TownOrCity, requestModel.County, requestModel.Postcode, venueID);

            var addedVenueModel = new AddedVenueModel
            {
                VenueName = HttpUtility.HtmlEncode(requestModel.VenueName),
                AddressLine1 = requestModel.AddressLine1,
                AddressLine2 = requestModel.AddressLine2,
                County = requestModel.County,
                Id = venueID,
                PostCode = requestModel.Postcode,
                Town = requestModel.TownOrCity
            };


            var returnUrl = _contextAccessor.HttpContext.Session.GetString("ADDNEWVENUERETURNURL");
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(new Url(returnUrl).SetQueryParam("venueId", venueID));
            }


            string option = _contextAccessor.HttpContext.Session.GetString("Option");

            if (option.ToUpper() == "ADDNEWVENUEFOREDIT")
            {
                _session.SetObject("NewAddedVenue", addedVenueModel);
                return RedirectToAction("Reload", "EditCourseRun");
            }

            if (option.ToUpper() == "ADDNEWVENUEFORCOPY")
            {
                _session.SetObject("NewAddedVenue", addedVenueModel);
                return RedirectToAction("Reload", "CopyCourseRun");
            }

            if (option.ToUpper() == "ADDNEWVENUE")
            {
                _session.SetObject("NewAddedVenue", addedVenueModel);
                return RedirectToAction("SummaryToAddCourseRun", "AddCourse");
            }
            if(option.ToUpper() == "ADDNEWVENUEFORAPPRENTICESHIPS")
            {
                _session.SetObject("NewAddedVenue", addedVenueModel);
                return RedirectToAction("DeliveryOptions", "Apprenticeships", new
                {
                    message = "",
                    mode = "Add"                    
                });
            }
            if (option.ToUpper() == "ADDNEWVENUEFORAPPRENTICESHIPSCOMBINED")
            {
                _session.SetObject("NewAddedVenue", addedVenueModel);
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
        public async Task<IActionResult> EditVenueName(EditVenueRequestModel requestModel)
        {
            AddressModel addressModel = null;

            if (!string.IsNullOrEmpty(requestModel.Id))
            {
                GetVenueByIdCriteria criteria = new GetVenueByIdCriteria(requestModel.Id);

                var getVenueByIdResult = await _venueService.GetVenueByIdAsync(criteria);
                if (getVenueByIdResult.IsSuccess && getVenueByIdResult.HasValue)
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

            } else {
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
            int? sUKPRN = _session.GetInt32("UKPRN");
            int UKPRN;
            if (!sUKPRN.HasValue)
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            else
                UKPRN = sUKPRN ?? 0;

            var result = _venueService.GetVenuesByPRNAndNameAsync(new GetVenuesByPRNAndNameCriteria(UKPRN.ToString(), VenueName)).Result;

            if (result.IsSuccess && result.Value.Value.Any(x => x.Status==Live))// && result.Value.Value.FirstOrDefault()?.Status == VenueStatus.Live)
                return Ok(true);
            return Ok(false);
        }

        [Authorize]
        public async Task<IActionResult> CheckForCoursesOrApprenticeships(Guid VenueId)
        {
            List<Course> Courses = new List<Course>();

            int? UKPRN = _session.GetInt32("UKPRN");
            if (!UKPRN.HasValue)
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            ICourseSearchResult coursesByUKPRN = (!UKPRN.HasValue
                   ? null
                   : _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                       .Result.Value);


            var apprenticeships = await _apprenticeshipService.GetApprenticeshipByUKPRN(UKPRN.ToString());
            var liveApprenticeships = apprenticeships.Value?.Where(x => x.RecordStatus == RecordStatus.Live);
            var apprenticeshipLocations = liveApprenticeships.SelectMany(x => x.ApprenticeshipLocations).Where(x => x.VenueId == VenueId && x.RecordStatus == RecordStatus.Live).ToList();

            Courses = coursesByUKPRN.Value.SelectMany(o => o.Value).SelectMany(i => i.Value).ToList();

            var liveCourseRuns = Courses.SelectMany(x => x.CourseRuns).Where(x => x.RecordStatus == Models.Enums.RecordStatus.Live && x.VenueId == VenueId).ToList();

            var migrationPendingCourseRuns = Courses.SelectMany(x => x.CourseRuns).Where(x => x.RecordStatus == Models.Enums.RecordStatus.MigrationPending && x.VenueId == VenueId).ToList();

            var bulkUploadPendingCourseRuns = Courses.SelectMany(x => x.CourseRuns).Where(x => x.RecordStatus == Models.Enums.RecordStatus.BulkUploadPending && x.VenueId == VenueId).ToList();

            var migrationReadyForLiveCourseRuns = Courses.SelectMany(x => x.CourseRuns).Where(x => x.RecordStatus == Models.Enums.RecordStatus.MigrationReadyToGoLive && x.VenueId == VenueId).ToList();

            var bulkUploadReadyForLiveCourseRuns = Courses.SelectMany(x => x.CourseRuns).Where(x => x.RecordStatus == Models.Enums.RecordStatus.BulkUploadReadyToGoLive && x.VenueId == VenueId).ToList();

            var model = new DeleteVenueCheckViewModel()
            {
                LiveCoursesExist = liveCourseRuns?.Count() > 0,
                PendingCoursesExist = migrationPendingCourseRuns?.Count() > 0 || bulkUploadPendingCourseRuns?.Count() > 0 || migrationReadyForLiveCourseRuns?.Count() > 0 || bulkUploadReadyForLiveCourseRuns?.Count() > 0,
                LiveApprenticeshipsExist = apprenticeshipLocations?.Count() > 0

            };

            return Ok(model);
        }

        [Authorize]
        public async Task<IActionResult> DeleteVenue(Guid VenueId)
        {
            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            IVenue updatedVenue = _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(VenueId.ToString())).Result.Value;
            updatedVenue.Status = Deleted;

            updatedVenue = _venueService.UpdateAsync(updatedVenue).Result.Value;

            VenueSearchResultItemModel deletedVenue = new VenueSearchResultItemModel(HttpUtility.HtmlEncode(updatedVenue.VenueName), updatedVenue.Address1, updatedVenue.Address2, updatedVenue.Town, updatedVenue.County, updatedVenue.PostCode, updatedVenue.ID);

            return View("VenueSearchResults", await GetVenues(deletedVenue));
        }

        [Authorize]
        [HttpGet]
        public IActionResult LandingOptions()
        {
            return View("../Venues/LandingOptions/Index", new LocationsLandingViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> LandingOptions(LocationsLandingViewModel model)
        {
            switch (model.LocationsLandingOptions)
            {
                case LocationsLandingOptions.Add:
                    return RedirectToAction("AddVenue", "Venues");
                case LocationsLandingOptions.Manage:
                    return RedirectToAction("Index", "Venues");
                default:
                    return RedirectToAction("LandingOptions", "Venues");
            }

        }
    }
}