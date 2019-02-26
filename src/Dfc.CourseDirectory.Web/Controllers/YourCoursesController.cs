using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.YourCourses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class YourCoursesController : Controller
    {
        private readonly ILogger<YourCoursesController> _logger;
        private readonly ISession _session;
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;

        public YourCoursesController(
            ILogger<YourCoursesController> logger,
            IHttpContextAccessor contextAccessor,
            ICourseService courseService,
            IVenueService venueService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(venueService, nameof(venueService));

            _logger = logger;
            _session = contextAccessor.HttpContext.Session;
            _courseService = courseService;
            _venueService = venueService;
        }

        public IActionResult Index()
        {
            var courseRuns = new List<CourseRunViewModel>
            {
                new CourseRunViewModel
                {
                    Id = Guid.NewGuid().ToString(),
                    CourseName = "Recruitment Resourcer (EPA)",
                    CourseId = "Some Course Id",
                    StartDate = DateTime.UtcNow.ToShortDateString(),
                    Url = "http://www.bbc.co.uk",
                    Cost = "£9999.99",
                    Duration = "999 Months",
                    DeliveryMode = "Classroom based",
                    Venue = "Matthew Boulton College, 1 Jennens Rd, Birmingham B4 7PS",
                    StudyMode = "Full-time",
                    AttendancePattern = "Day/Block Release"
                },
                new CourseRunViewModel
                {
                    Id = Guid.NewGuid().ToString(),
                    CourseName = "Recruitment Resourcer (EPA)",
                    CourseId = "Some Course Id",
                    StartDate = DateTime.UtcNow.ToShortDateString(),
                    Url = "http://www.bbc.co.uk",
                    Cost = "£9999.99",
                    Duration = "999 Months",
                    DeliveryMode = "Online"
                },
                new CourseRunViewModel
                {
                    Id = Guid.NewGuid().ToString(),
                    CourseName = "Recruitment Resourcer (EPA)",
                    CourseId = "Some Course Id",
                    StartDate = DateTime.UtcNow.ToShortDateString(),
                    Url = "http://www.bbc.co.uk",
                    Cost = "£9999.99",
                    Duration = "999 Months",
                    DeliveryMode = "Work based",
                    Region = "North East, North West, Yorkshire and Humberside, West Midlands, East Midlands, South East, South West, London"
                }
            };
            var course1 = new CourseViewModel
            {
                Id = Guid.NewGuid().ToString(),
                LearnAimRef = "10003385",
                NotionalNVQLevelv2 = "2",
                QualificationTitle = "Recruitment Resourcer (EPA)",
                AwardOrg = "REC",
                CourseRuns = courseRuns
            };
            var course2 = new CourseViewModel
            {
                Id = Guid.NewGuid().ToString(),
                LearnAimRef = "10003385",
                NotionalNVQLevelv2 = "2",
                QualificationTitle = "Recruitment Resourcer (EPA)",
                CourseRuns = courseRuns
            };
            var course3 = new CourseViewModel
            {
                Id = Guid.NewGuid().ToString(),
                LearnAimRef = "10003385",
                NotionalNVQLevelv2 = "2",
                QualificationTitle = "Recruitment Resourcer (EPA)",
                CourseRuns = courseRuns
            };

            var viewModel = new YourCoursesViewModel
            {
                Heading = "Level 2",
                HeadingCaption = "Your courses",
                Courses = new List<CourseViewModel> { course1, course2, course3 }
            };

            return View(viewModel);
        }
    }
}