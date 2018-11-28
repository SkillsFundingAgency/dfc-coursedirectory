using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewComponents.VenueList;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class VenueSearchController : Controller
    {
        private readonly ILogger<LarsSearchController> _logger;

        public VenueSearchController(
            ILogger<LarsSearchController> logger)
        {
            Throw.IfNull(logger, nameof(logger));

            _logger = logger;
        }

        public async Task<IActionResult> Index([FromQuery] string SearchTerm)
        {
            VenueListModel model = new VenueListModel();

            if (string.IsNullOrEmpty(SearchTerm))
            {
           
            }
            else
            {
                //TODO SERVICE CALL??


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

                model.Venues = venues.ToList();
            }

            return ViewComponent(nameof(ViewComponents.VenueList), model);
        }
    }
}