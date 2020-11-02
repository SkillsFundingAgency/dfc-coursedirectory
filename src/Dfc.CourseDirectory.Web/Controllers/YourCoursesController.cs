using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Models.Regions;
using Dfc.CourseDirectory.Services.Models.Venues;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.ViewModels.YourCourses;
using Microsoft.AspNetCore.Authorization;
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

        internal Venue GetVenueByIdFrom(IEnumerable<Venue> list, Guid id)
        {
            if (list == null) list = new List<Venue>();

            var found = list.ToList().Find(x => x.ID == id.ToString());

            return found;
        }

        internal string FormatAddress(Venue venue)
        {
            if (venue == null) return string.Empty;

            var list = new List<string>
            {
                venue.Address1,
                venue.Address2,
                venue.Address3,
                venue.County,
                venue.PostCode
            }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

            return string.Join(", ", list);
        }

        internal string FormattedRegionsByIds(IEnumerable<RegionItemModel> list, IEnumerable<string> ids)
        {
            if (list == null) list = new List<RegionItemModel>();
            if (ids == null) ids = new List<string>();

            ids = ids
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x));

            if (ids.Count() == 0) return string.Empty;

            var matching = list
                .Where(x => ids.Contains(x.Id))
                .Select(x => x.RegionName);

            return string.Join(", ", matching);
        }

        internal string CourseNameByCourseRunId(IEnumerable<CourseRunViewModel> courseRuns, string id)
        {
            if (courseRuns == null) return string.Empty;
            if (string.IsNullOrWhiteSpace(id)) return string.Empty;

            var found = courseRuns.FirstOrDefault(x => x.Id == id)?.CourseName;

            return found;
        }

        internal string QualificationTitleByCourseId(IEnumerable<CourseViewModel> courses, string id)
        {
            if (courses == null) return string.Empty;
            if (string.IsNullOrWhiteSpace(id)) return string.Empty;

            var found = courses.FirstOrDefault(x => x.Id == id)?.QualificationTitle;

            return found;
        }
        [Authorize]
        public async Task<IActionResult> Index(
            string level, 
            Guid? courseId, 
            Guid? courseRunId, 
            string notificationTitle, 
            string notificationMessage)
        {
            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var courseResult = (await _courseService.GetCoursesByLevelForUKPRNAsync(new CourseSearchCriteria(UKPRN))).Value;
            var venueResult = (await _venueService.SearchAsync(new VenueSearchCriteria(UKPRN.ToString(), string.Empty))).Value;
            var allRegions = _courseService.GetRegions().RegionItems;

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
                    QualificationType = x.QualificationType,
                    CourseRuns = x.CourseRuns.Select(y => new CourseRunViewModel
                    {
                        Id = y.id.ToString(),
                        CourseId = y.ProviderCourseID,
                        AttendancePattern = y.AttendancePattern.ToDescription(),
                        Cost = y.Cost.HasValue ? $"£ {y.Cost.Value:0.00}" : string.Empty,
                        CourseName = y.CourseName,
                        DeliveryMode = y.DeliveryMode.ToDescription(),
                        Duration = y.DurationValue.HasValue ? $"{y.DurationValue.Value} {y.DurationUnit.ToDescription()}" : $"0 {y.DurationUnit.ToDescription()}",
                        Venue = y.VenueId.HasValue ? FormatAddress(GetVenueByIdFrom(venueResult.Value, y.VenueId.Value)) : string.Empty,
                        Region =  y.Regions != null ? FormattedRegionsByIds(allRegions, y.Regions) : string.Empty,
                        StartDate = y.FlexibleStartDate ? "Flexible start date" : y.StartDate?.ToString("dd/mm/yyyy"),
                        StudyMode = y.StudyMode.ToDescription(),
                        Url = y.CourseURL
                    })
                    .OrderBy(y => y.CourseName)
                    .ToList()
                })
                .OrderBy(x => x.QualificationTitle)
                .ToList();

            var notificationCourseName = string.Empty;
            var notificationAnchorTag = string.Empty;

            if (courseId.HasValue && courseId.Value != Guid.Empty)
            {
                notificationCourseName = QualificationTitleByCourseId(courseViewModels, courseId.ToString());

                if (courseRunId.HasValue && courseRunId.Value != Guid.Empty)
                {
                    var courseRuns = courseViewModels.Find(x => x.Id == courseId.Value.ToString())?.CourseRuns;
                    notificationCourseName = Regex.Replace(CourseNameByCourseRunId(courseRuns, courseRunId.ToString()), "<.*?>", String.Empty);
                }
            }

            notificationAnchorTag = courseRunId.HasValue 
                ? $"<a id=\"courseeditlink\" class=\"govuk-link\" href=\"#\" data-courseid=\"{courseId}\" data-courserunid=\"{courseRunId}\" >{notificationCourseName} {notificationMessage}</a>" 
                : $"<a id=\"courseeditlink\" class=\"govuk-link\" href=\"#\" data-courseid=\"{courseId}\">{notificationCourseName} {notificationMessage}</a>";

            var viewModel = new YourCoursesViewModel
            {
                Heading = $"Level {level}",
                HeadingCaption = "Your courses",
                Courses = courseViewModels ?? new List<CourseViewModel>(),
                LevelFilters = levelFilters,
                NotificationTitle = notificationTitle,
                NotificationMessage = notificationAnchorTag
            };

            return View(viewModel);
        }
    }
}