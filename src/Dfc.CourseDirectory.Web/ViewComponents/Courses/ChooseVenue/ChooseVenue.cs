using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Venues;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseVenue
{
    public class ChooseVenue : ViewComponent
    {
        private readonly IVenueSearchHelper _venueSearchHelper;
        private readonly IVenueService _venueService;
        private ISession Session => HttpContext.Session;
        public ChooseVenue(IVenueSearchHelper venueSearchHelper, IVenueService venueService)
        {
            if (venueService == null)
            {
                throw new ArgumentNullException(nameof(venueService));
            }

            _venueSearchHelper = venueSearchHelper;
            _venueService = venueService;
        }

        public async Task<IViewComponentResult> InvokeAsync(ChooseVenueModel model)
        {
            List<SelectListItem> venues = new List<SelectListItem>();

            var UKPRN = Session.GetInt32("UKPRN");
            if (UKPRN.HasValue)
            {
                var requestModel = new VenueSearchRequestModel { SearchTerm = Session.GetInt32("UKPRN").Value.ToString() };
                var criteria = _venueSearchHelper.GetVenueSearchCriteria(requestModel);
                var result = await _venueService.SearchAsync(criteria);

                if (result.IsSuccess)
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
