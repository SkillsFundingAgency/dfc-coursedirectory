using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.AddressSelectionConfirmation;
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

            
            //return ViewComponent(nameof(ViewComponents.VenueSearchResult.VenueSearchResult ), resultModel);

            return View("VenueSearchResults", resultModel);
        }


    }
}