using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult;
using Dfc.CourseDirectory.Web.ViewComponents.Venue;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.Venue
{
    public class VenueList : ViewComponent
    {
        public IViewComponentResult Invoke(VenueListModel model)
        {
            var actualModel = model ?? new VenueListModel();

            var venues = new List<VenueItemModel>();

            var venue = new VenueItemModel()
            {
                VenueName = "Stratford campus",
                AddressLine1 = "Welfare Road",
                AddressLine2 = "Stratford",
                AddressLine3 = "Greater London",
                PostCode = "E15 4H"
            };

            venues.Add(venue);

            venue = new VenueItemModel()
            {
                VenueName = "Eastleigh campus",
                AddressLine1 = "Chestnut Avenue",
                AddressLine2 = "Eastleigh",
                AddressLine3 = "Hampshire",
                PostCode = "SO50 5FS"
            };

            venues.Add(venue);

            venue = new VenueItemModel()
            {
                VenueName = "Salford campus",
                AddressLine1 = "Lissadel Street",
                AddressLine2 = "Salford",
                PostCode = "M6 6AP"
            };

            venues.Add(venue);

            actualModel.Venues = venues.ToList();

            return View("~/ViewComponents/VenueList/Default.cshtml", actualModel);
        }
    }
}