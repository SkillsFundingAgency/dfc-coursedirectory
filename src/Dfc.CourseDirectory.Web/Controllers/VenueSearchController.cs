using System.Threading.Tasks;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
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
        private readonly IVenueServiceSettings _venueServiceSettings;
        private readonly IVenueService _venueService;
        private readonly IVenueSearchHelper _venueSearchHelper;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public VenueSearchController(
            ILogger<VenueSearchController> logger,
            IOptions<VenueServiceSettings> venueServiceSettings,
            IVenueService venueService,
            IVenueSearchHelper venueSearchHelper,
            IHttpContextAccessor contextAccessor)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(venueServiceSettings, nameof(venueServiceSettings));
            Throw.IfNull(venueService, nameof(venueService));

            _logger = logger;
            _venueServiceSettings = venueServiceSettings.Value;
            _venueService = venueService;
            _venueSearchHelper = venueSearchHelper;
            _contextAccessor = contextAccessor;
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            int UKPRN = 0;
            if (_session.GetInt32("UKPRN").HasValue)
            {
                UKPRN = _session.GetInt32("UKPRN").Value;
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

            _logger.LogInformationObject("Model", requestModel);

            if (requestModel == null)
            {
                model = new VenueSearchResultModel();
            }
            else
            {
                var criteria = _venueSearchHelper.GetVenueSearchCriteria(requestModel);

                var result = await _venueService.SearchAsync(criteria);

                if(result.IsSuccess && result.HasValue)
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
