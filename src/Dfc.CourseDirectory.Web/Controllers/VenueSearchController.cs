using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class VenueSearchController : Controller
    {
        private readonly ILogger<VenueSearchController> _logger;
        private readonly VenueServiceSettings _venueServiceSettings;
        private readonly IVenueService _venueService;
        private readonly IVenueSearchHelper _venueSearchHelper;
        private ISession Session => HttpContext.Session;

        public VenueSearchController(
            ILogger<VenueSearchController> logger,
            IOptions<VenueServiceSettings> venueServiceSettings,
            IVenueService venueService,
            IVenueSearchHelper venueSearchHelper)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (venueServiceSettings == null)
            {
                throw new ArgumentNullException(nameof(venueServiceSettings));
            }

            if (venueService == null)
            {
                throw new ArgumentNullException(nameof(venueService));
            }

            _logger = logger;
            _venueServiceSettings = venueServiceSettings.Value;
            _venueService = venueService;
            _venueSearchHelper = venueSearchHelper;
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            int UKPRN = 0;
            if (Session.GetInt32("UKPRN").HasValue)
            {
                UKPRN = Session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            VenueSearchRequestModel requestModel = new VenueSearchRequestModel
            {
                SearchTerm = UKPRN.ToString()
            };
            VenueSearchResultModel model;

            if (requestModel == null)
            {
                model = new VenueSearchResultModel();
            }
            else
            {
                var criteria = _venueSearchHelper.GetVenueSearchCriteria(requestModel);

                var result = await _venueService.SearchAsync(criteria);

                if (result.IsSuccess)
                {
                    var items = _venueSearchHelper.GetVenueSearchResultItemModels(result.Value.Value);
                    model = new VenueSearchResultModel(
                        requestModel.SearchTerm, 
                        items,null,false);
                }
                else
                {
                    model = new VenueSearchResultModel(result.Error);
                }
            }

            return ViewComponent(nameof(ViewComponents.VenueSearchResult.VenueSearchResult), model);
            
        }
    }
}
