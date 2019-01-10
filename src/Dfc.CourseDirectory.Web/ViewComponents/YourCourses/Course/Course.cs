using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewComponents.YourCourses.Course
{
    public class Course : ViewComponent
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private IVenueService _service;

        public Course(
            IHttpContextAccessor contextAccessor, IVenueService venueService)
        {
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(venueService, nameof(venueService));

            _contextAccessor = contextAccessor;
            _service = venueService;

        }

        public async Task<IViewComponentResult> InvokeAsync(YourCoursesViewModel model)
        {
            //var courseRunVenues = new List<SelectListItem>();

            //var UKPRN = _session.GetInt32("UKPRN");
            //if (UKPRN.HasValue)
            //{
            //    VenueSearchCriteria criteria = new VenueSearchCriteria(UKPRN.ToString(), null);

            //    var venues = await _service.SearchAsync(criteria);

            //    foreach (var venue in venues.Value.Value)
            //    {
            //        var item = new SelectListItem
            //            { Text = venue.VenueName, Value = venue.ID };

            //        courseRunVenues.Add(item);
            //    };

            //    model.Venues = courseRunVenues;
            //}

           

            return View("~/ViewComponents/YourCourses/Course/Default.cshtml", model);
        }
    }
}
