using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
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

        public async Task<IActionResult> Index(string level)
        {
            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var courseResult = _courseService.GetCoursesByLevelForUKPRNAsync(new CourseSearchCriteria(UKPRN)).Result.Value;

            var levelFilters = courseResult
                .Value
                .Select(x => new QualificationLevelFilterViewModel
                {
                    Name = $"Level {x.Level}",
                    Value = x.Level,
                    Facet = x.Value.Count().ToString(),
                    IsSelected = level == x.Level
                })
                .OrderBy(x => x.Value)
                .ToList();

            if (string.IsNullOrWhiteSpace(level))
            {
                level = levelFilters.FirstOrDefault()?.Value;
                levelFilters.ForEach(x => { if (x.Value == level) { x.IsSelected = true; } });
            }

            var courseViewModels = courseResult
                .Value
                .SingleOrDefault(x => x.Level == level)
                ?.Value
                .SelectMany(x => x.Value)
                .Select(x => new CourseViewModel
                {
                    Id = x.id.ToString(),
                    AwardOrg = x.AwardOrgCode,
                    LearnAimRef = x.LearnAimRef,
                    NotionalNVQLevelv2 = x.NotionalNVQLevelv2,
                    QualificationTitle = x.QualificationCourseTitle,
                    CourseRuns = x.CourseRuns.Select(y => new CourseRunViewModel
                    {
                        Id = y.id.ToString(),
                        CourseId = y.ProviderCourseID,
                        AttendancePattern = y.AttendancePattern.ToDescription(),
                        Cost = y.Cost.HasValue ? $"£{y.Cost.Value.ToString()}" : string.Empty,
                        CourseName = y.CourseName,
                        DeliveryMode = y.DeliveryMode.ToDescription(),
                        Duration = y.DurationValue.HasValue ? $"{y.DurationValue.Value} {y.DurationUnit.ToDescription()}" : $"0 {y.DurationUnit.ToDescription()}",
                        Venue = y.VenueId.HasValue ? y.VenueId.Value.ToString() : string.Empty,
                        Region =  y.Regions != null ? string.Join(", ", y.Regions) : string.Empty,
                        StartDate = y.FlexibleStartDate ? "Flexible start date" : y.StartDate?.ToString("dd/mm/yyyy"),
                        StudyMode = y.StudyMode.ToDescription(),
                        Url = y.CourseURL
                    })
                    .ToList()
                })
                .ToList();

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
            var filters = new List<QualificationLevelFilterViewModel>
            {
                new QualificationLevelFilterViewModel
                {
                    Name = "Entry Level",
                    Facet = "888",
                    Value = "Entry",
                    IsSelected = false
                },
                new QualificationLevelFilterViewModel
                {
                    Name = "Level 1",
                    Facet = "888",
                    Value = "1",
                    IsSelected = false
                },
                new QualificationLevelFilterViewModel
                {
                    Name = "Level 2",
                    Facet = "888",
                    Value = "2",
                    IsSelected = true
                },
                new QualificationLevelFilterViewModel
                {
                    Name = "Level 3",
                    Facet = "888",
                    Value = "3",
                    IsSelected = false
                },
                new QualificationLevelFilterViewModel
                {
                    Name = "Level 4",
                    Facet = "888",
                    Value = "4",
                    IsSelected = false
                },
                new QualificationLevelFilterViewModel
                {
                    Name = "Level 5",
                    Facet = "888",
                    Value = "5",
                    IsSelected = false
                },
                new QualificationLevelFilterViewModel
                {
                    Name = "Level 6",
                    Facet = "888",
                    Value = "6",
                    IsSelected = false
                },
                new QualificationLevelFilterViewModel
                {
                    Name = "Level 7",
                    Facet = "888",
                    Value = "7",
                    IsSelected = false
                },
                new QualificationLevelFilterViewModel
                {
                    Name = "Level 8",
                    Facet = "888",
                    Value = "8",
                    IsSelected = false
                }
            };

            var viewModel = new YourCoursesViewModel
            {
                Heading = $"Level {level}",
                HeadingCaption = "Your courses",
                Courses = courseViewModels ?? new List<CourseViewModel>(),
                LevelFilters = levelFilters
            };

            return View(viewModel);
        }
    }
}