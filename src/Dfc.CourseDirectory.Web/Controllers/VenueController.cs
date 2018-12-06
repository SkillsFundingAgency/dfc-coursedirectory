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

        public VenueController(
            ILogger<VenueController> logger,
            IPostCodeSearchService postCodeSearchService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(postCodeSearchService, nameof(postCodeSearchService));
            _logger = logger;
            _postCodeSearchService = postCodeSearchService;
        }

        public IActionResult Index()
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
    }
}