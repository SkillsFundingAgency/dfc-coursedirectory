using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Models.Models.Venues;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseVenue
{
    public class ChooseVenue : ViewComponent
    {
        private readonly IVenueSearchHelper _venueSearchHelper;
        private readonly IVenueService _venueService;
        private readonly IVenueServiceSettings _venueServiceSettings;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;
        public ChooseVenue(IVenueSearchHelper venueSearchHelper, IVenueService venueService, IOptions<VenueServiceSettings> venueServiceSettings, IHttpContextAccessor contextAccessor)
        {
            Throw.IfNull(venueService, nameof(venueService));
            Throw.IfNull(venueServiceSettings, nameof(venueServiceSettings));

            _venueServiceSettings = venueServiceSettings.Value;
            _venueSearchHelper = venueSearchHelper;
            _venueService = venueService;
            _contextAccessor = contextAccessor;
        }
        public async Task<IViewComponentResult> InvokeAsync(ChooseVenueModel model)
        {
            List<SelectListItem> venues = new List<SelectListItem>();

            var UKPRN = _session.GetInt32("UKPRN");
            if (UKPRN.HasValue)
            {
                var requestModel = new VenueSearchRequestModel { SearchTerm = _session.GetInt32("UKPRN").Value.ToString() };
                var criteria = _venueSearchHelper.GetVenueSearchCriteria(requestModel);
                var result = await _venueService.SearchAsync(criteria);

                if (result.IsSuccess && result.HasValue)
                {
                    var defaultItem = new SelectListItem { Text = "Select", Value = "" };

                    foreach (var venue in result.Value.Value.Where(x => x.Status == VenueStatus.Live))
                    {
                        var item = new SelectListItem { Text = venue.VenueName, Value = venue.ID };
                        venues.Add(item);
                    };

                    venues.Insert(0,defaultItem);
                }

            }

            model.Venues = venues;
            return View("~/ViewComponents/Courses/ChooseVenue/Default.cshtml", model);

        }
    }
}
