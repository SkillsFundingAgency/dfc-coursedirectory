using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.ViewComponents.PostcodeLookup;
using Dfc.CourseDirectory.WebV2.LoqateAddressSearch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class PostcodeLookupController : Controller
    {
        private readonly ILogger<PostcodeLookupController> _logger;
        private readonly IAddressSearchService _addressSearchService;
        private readonly IVenueService _venueService;
        private readonly ISession _session;

        public PostcodeLookupController(
            ILogger<PostcodeLookupController> logger,
            IAddressSearchService addressSearchService,
            IVenueService venueService,
            IHttpContextAccessor contextAccessor)
        {
            _logger = logger;
            _addressSearchService = addressSearchService;
            _venueService = venueService;
            _session = contextAccessor.HttpContext.Session;
        }
        [Authorize]
        public async Task<IActionResult> Index(string postcode, string venuename, string id)
        {
            var result = await _addressSearchService.SearchByPostcode(postcode);

            var listItems = new List<SelectListItem>();

            if (result.Count == 0)
            {
                listItems = null;
            }
            else
            {

                foreach (var item in result)
                {
                    listItems.Add(new SelectListItem(item.StreetAddress, item.Id));
                }

            }

            var model = new PostcodeLookupModel
            {
                Id = id,
                VenueName = venuename,
                PostcodeLabelText = "Postcode",
                Postcode = postcode,
                Items = listItems,
                Searched = true,
                ButtonText = "Find address",
            };

            return ViewComponent(nameof(ViewComponents.PostcodeLookup.PostcodeLookup), model);
        }
        [Authorize]
        public IActionResult Default(string venuename, string id)
        {


            return ViewComponent(nameof(ViewComponents.PostcodeLookup.PostcodeLookup), new PostcodeLookupModel
            {
                Id = id,
                VenueName = venuename,
                PostcodeLabelText = "Postcode",
                PostcodeAriaDescribedBy = "Please enter the postcode.",
                ButtonText = "Find address"
            });
        }
    }
}
