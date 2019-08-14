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
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces;
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
        private readonly IPostCodeSearchService _postCodeSearchService;
        private readonly IVenueServiceSettings _venueServiceSettings;
        private readonly IVenueSearchHelper _venueSearchHelper;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IVenueService _venueService;
        private readonly IOnspdSearchHelper _onspdSearchHelper;
        private readonly ICourseService _courseService;

        private ISession _session => _contextAccessor.HttpContext.Session;



        public LocationsController(
            ILogger<LocationsController> logger,
            IPostCodeSearchService postCodeSearchService,
            IOptions<VenueServiceSettings> venueSearchSettings,
            IVenueSearchHelper venueSearchHelper,
            IHttpContextAccessor contextAccessor,
            IVenueService venueService,
            IOnspdSearchHelper onspdSearchHelper,
            ICourseService courseService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(postCodeSearchService, nameof(postCodeSearchService));
            Throw.IfNull(venueSearchSettings, nameof(venueSearchSettings));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(venueService, nameof(venueService));
            Throw.IfNull(courseService, nameof(courseService));

            _logger = logger;
            _postCodeSearchService = postCodeSearchService;
            _venueServiceSettings = venueSearchSettings.Value;
            _venueSearchHelper = venueSearchHelper;
            _contextAccessor = contextAccessor;
            _venueService = venueService;
            _onspdSearchHelper = onspdSearchHelper;
            _courseService = courseService;
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DeleteLocation(Guid VenueId)
        {
            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            LocationDeleteViewModel locationDeleteViewModel = new LocationDeleteViewModel();
            locationDeleteViewModel.VenueId = VenueId;

            return View("../Venues/locationdelete/index", locationDeleteViewModel);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteLocation(LocationDeleteViewModel locationDeleteViewModel)
        {
            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            IVenue updatedVenue = _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(locationDeleteViewModel.VenueId.ToString())).Result.Value;
            updatedVenue.Status = VenueStatus.Deleted;

            updatedVenue = _venueService.UpdateAsync(updatedVenue).Result.Value;

            VenueSearchResultItemModel deletedVenue = new VenueSearchResultItemModel(HttpUtility.HtmlEncode(updatedVenue.VenueName), updatedVenue.Address1, updatedVenue.Address2, updatedVenue.Town, updatedVenue.County, updatedVenue.PostCode, updatedVenue.ID);

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





    }
}