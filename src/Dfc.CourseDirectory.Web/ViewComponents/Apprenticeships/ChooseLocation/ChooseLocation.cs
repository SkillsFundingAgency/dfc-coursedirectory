using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Models.Venues;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships
{
    public class ChooseLocation : ViewComponent
    {
        private readonly IVenueSearchHelper _venueSearchHelper;
        private readonly IVenueService _venueService;
        private readonly VenueServiceSettings _venueServiceSettings;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;
        public ChooseLocation(IVenueSearchHelper venueSearchHelper, IVenueService venueService, IOptions<VenueServiceSettings> venueServiceSettings, IHttpContextAccessor contextAccessor)
        {
            Throw.IfNull(venueService, nameof(venueService));
            Throw.IfNull(venueServiceSettings, nameof(venueServiceSettings));

            _venueServiceSettings = venueServiceSettings.Value;
            _venueSearchHelper = venueSearchHelper;
            _venueService = venueService;
            _contextAccessor = contextAccessor;
        }
        public async Task<IViewComponentResult> InvokeAsync(ChooseLocationModel model)
        {
            List<SelectListItem> venues = new List<SelectListItem>();

            var UKPRN = _session.GetInt32("UKPRN");
            if (UKPRN.HasValue)
            {
                var requestModel = new VenueSearchRequestModel { SearchTerm = _session.GetInt32("UKPRN").Value.ToString() };
                var criteria = _venueSearchHelper.GetVenueSearchCriteria(requestModel);
                var result = await _venueService.SearchAsync(criteria);

                if (result.IsSuccess)
                {
                    //var defaultItem = new SelectListItem { Text = "Select", Value = "",Selected=true };

                    foreach (var venue in result.Value.Value.Where(x => x.Status == VenueStatus.Live))
                    {
                        var item = new SelectListItem { Text = venue.VenueName, Value = venue.ID };

                        if (model.DeliveryLocations == null || !model.DeliveryLocations.Any(x => x.LocationId.HasValue && x.LocationId.Value.ToString() == item.Value))
                        {
                            venues.Add(item);
                        }
                    };

                }

            }

            model.Locations = venues;
            return View("~/ViewComponents/Apprenticeships/ChooseLocation/Default.cshtml", model);

        }
    }
}
