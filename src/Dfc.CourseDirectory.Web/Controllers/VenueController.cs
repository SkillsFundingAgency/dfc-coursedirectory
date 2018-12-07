using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.ViewComponents.AddressSelectionConfirmation;
using Dfc.CourseDirectory.Web.ViewComponents.PostCodeSearchResult;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class VenueController : Controller
    {
        private readonly ILogger<VenueController> _logger;
        private readonly IPostCodeSearchService _postCodeSearchService;
        private readonly IVenueAddService _venueAddService;

        public VenueController(
            ILogger<VenueController> logger,
            IPostCodeSearchService postCodeSearchService,
                IVenueAddService venueAddService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(postCodeSearchService, nameof(postCodeSearchService));
            Throw.IfNull(venueAddService, nameof(venueAddService));
            _logger = logger;
            _postCodeSearchService = postCodeSearchService;
            _venueAddService = venueAddService;
        }

        public async Task<IActionResult> Index()
        {
            VenueAdd venue= new VenueAdd("Address1","Address2","town","venuename","county","b71 4du");
          var t= await _venueAddService.AddAsync(venue);
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
        public async Task<IActionResult> SaveVenue(PostCodeSearchResultModel model)
        {

          

            return View("index", model);
        }
        public IActionResult EditVenueName()
        {
            return View();
        }
    }
}