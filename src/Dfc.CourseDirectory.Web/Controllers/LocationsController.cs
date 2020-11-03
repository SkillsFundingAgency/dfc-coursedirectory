using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.Models.Venues;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.Locations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class LocationsController : Controller
    {
        private readonly ILogger<LocationsController> _logger;
        private readonly IVenueServiceSettings _venueServiceSettings;
        private readonly IVenueSearchHelper _venueSearchHelper;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IVenueService _venueService;
        private readonly IOnspdSearchHelper _onspdSearchHelper;
        private readonly ICourseService _courseService;

        private ISession _session => _contextAccessor.HttpContext.Session;

        public LocationsController(
            ILogger<LocationsController> logger,
            IOptions<VenueServiceSettings> venueSearchSettings,
            IVenueSearchHelper venueSearchHelper,
            IHttpContextAccessor contextAccessor,
            IVenueService venueService,
            IOnspdSearchHelper onspdSearchHelper,
            ICourseService courseService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(venueSearchSettings, nameof(venueSearchSettings));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(venueService, nameof(venueService));
            Throw.IfNull(courseService, nameof(courseService));

            _logger = logger;
            _venueServiceSettings = venueSearchSettings.Value;
            _venueSearchHelper = venueSearchHelper;
            _contextAccessor = contextAccessor;
            _venueService = venueService;
            _onspdSearchHelper = onspdSearchHelper;
            _courseService = courseService;
        }

        [Authorize]
        [HttpGet]
        public IActionResult DeleteLocation(Guid VenueId)
        {
            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var venueResult = _venueService
                .GetVenueByIdAsync(new GetVenueByIdCriteria(VenueId.ToString())).Result
                .Value;

            LocationDeleteViewModel locationDeleteViewModel = new LocationDeleteViewModel();
            locationDeleteViewModel.VenueId = VenueId;
            locationDeleteViewModel.VenueName = venueResult.VenueName;
            locationDeleteViewModel.PostCode = venueResult.PostCode;
            locationDeleteViewModel.AddressLine1 = venueResult.Address1;

            return View("../Venues/locationdelete/index", locationDeleteViewModel);
        }

        [Authorize]
        [HttpPost]
        public IActionResult DeleteLocation(LocationDeleteViewModel locationDeleteViewModel)
        {
            if (locationDeleteViewModel.LocationDelete == LocationDelete.Delete)
            {
                int? UKPRN = _session.GetInt32("UKPRN");

                if (!UKPRN.HasValue)
                {
                    return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
                }

                var updatedVenue = _venueService
                    .GetVenueByIdAsync(new GetVenueByIdCriteria(locationDeleteViewModel.VenueId.ToString())).Result
                    .Value;
                updatedVenue.Status = VenueStatus.Deleted;

                updatedVenue = _venueService.UpdateAsync(updatedVenue).Result.Value;

                VenueSearchResultItemModel deletedVenue = new VenueSearchResultItemModel(
                    HtmlEncoder.Default.Encode(updatedVenue.VenueName), updatedVenue.Address1, updatedVenue.Address2,
                    updatedVenue.Town, updatedVenue.County, updatedVenue.PostCode, updatedVenue.ID);

                return RedirectToAction("LocationConfirmationDelete", "Locations",new{VenueId = updatedVenue.ID });
            }

            return RedirectToAction("Index", "Venues");
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

            var viewModel = new VenueSearchResultsViewModel
            {
                Result = model
            };
            return viewModel;
        }

        [Authorize]
        [HttpGet]
        public IActionResult LocationConfirmationDelete(Guid VenueId)
        {
            var venueResult = _venueService
                .GetVenueByIdAsync(new GetVenueByIdCriteria(VenueId.ToString())).Result
                .Value;

            LocationDeleteConfirmViewModel locationDeleteConfirmViewModel = new LocationDeleteConfirmViewModel();
            locationDeleteConfirmViewModel.VenueId = VenueId;
            locationDeleteConfirmViewModel.VenueName = venueResult.VenueName;
            locationDeleteConfirmViewModel.PostCode = venueResult.PostCode;
            locationDeleteConfirmViewModel.AddressLine1 = venueResult.Address1;

            return View("../Venues/LocationDeleteConfirmation/index", locationDeleteConfirmViewModel);
        }

        [Authorize]
        [HttpPost]
        public IActionResult LocationConfirmationDelete()
        {
            return RedirectToAction("Index", "Venues");
        }
    }
}
