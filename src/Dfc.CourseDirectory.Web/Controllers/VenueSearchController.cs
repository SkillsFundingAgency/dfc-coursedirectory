using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class VenueSearchController : Controller
    {
        private readonly ILogger<VenueSearchController> _logger;
        private readonly IVenueSearchSettings _venueSearchSettings;
        private readonly IVenueSearchService _venueSearchService;
        private readonly IVenueSearchHelper _venueSearchHelper;

        public VenueSearchController(
            ILogger<VenueSearchController> logger,
            IOptions<VenueSearchSettings> venueSearchSettings,
            IVenueSearchService venueSearchService,
            IVenueSearchHelper venueSearchHelper)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(venueSearchSettings, nameof(venueSearchSettings));
            Throw.IfNull(venueSearchService, nameof(venueSearchService));

            _logger = logger;
            _venueSearchSettings = venueSearchSettings.Value;
            _venueSearchService = venueSearchService;
            _venueSearchHelper = venueSearchHelper;
        }
        public async Task<IActionResult> Index([FromQuery] VenueSearchRequestModel requestModel)
        {
            VenueSearchResultModel model;

            _logger.LogMethodEnter();
            _logger.LogInformationObject("Model", requestModel);

            if (requestModel == null)
            {
                model = new VenueSearchResultModel();
            }
            else
            {
                var criteria = _venueSearchHelper.GetVenueSearchCriteria(
                    requestModel);

                var result = await _venueSearchService.SearchAsync(criteria);

                if(result.IsSuccess && result.HasValue)
                {
                    var items = _venueSearchHelper.GetVenueSearchResultItemModels(result.Value.Value);
                    model = new VenueSearchResultModel(
                        requestModel.SearchTerm, 
                        items,null);
                }
                else
                {
                    model = new VenueSearchResultModel(result.Error);
                }
            }

            _logger.LogMethodExit();
            return ViewComponent(nameof(ViewComponents.VenueSearchResult.VenueSearchResult), model);
            
        }
    }
}