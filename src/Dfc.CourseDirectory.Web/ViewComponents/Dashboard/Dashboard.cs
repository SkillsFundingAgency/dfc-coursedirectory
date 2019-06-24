using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Dashboard
{
    public class Dashboard : ViewComponent
    {
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public Dashboard(ICourseService courseService, IVenueService venueService, IHttpContextAccessor contextAccessor)
        {
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(venueService, nameof(venueService));


            _courseService = courseService;
            _venueService = venueService;
            _contextAccessor = contextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync(DashboardModel model)
        {
            var actualModel = model ?? new DashboardModel();

            int UKPRN = 0;
            if (_session.GetInt32("UKPRN").HasValue)
            {
                UKPRN = _session.GetInt32("UKPRN").Value;
            }

            var allVenues = await _venueService.SearchAsync(new VenueSearchCriteria(UKPRN.ToString(), ""));
            
            IEnumerable<Course> courses = _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                                               .Result
                                               .Value
                                               .Value
                                               .SelectMany(o => o.Value)
                                               .SelectMany(i => i.Value);

            actualModel.VenueCount = 0;
            if (allVenues != null)
            {
                actualModel.VenueCount = allVenues.Value.Value.Count();
            }

            actualModel.PublishedCourseCount = courses.Where(x => x.CourseStatus == RecordStatus.Live)
                                                 .SelectMany(c => c.CourseRuns)
                                                 .Where(x => x.RecordStatus == RecordStatus.Live)
                                                 .Count();

            return View("~/ViewComponents/Dashboard/Default.cshtml", actualModel);
        }
    }
}
