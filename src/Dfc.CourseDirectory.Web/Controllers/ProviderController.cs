using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Services.Models.Providers;
using Dfc.CourseDirectory.Services.Models.Regions;
using Dfc.CourseDirectory.Services.Models.Venues;
using Dfc.CourseDirectory.Services.ProviderService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.Helpers.Attributes;
using Dfc.CourseDirectory.Web.ViewModels.Provider;
using Dfc.CourseDirectory.Web.ViewModels.YourCourses;
using Dfc.CourseDirectory.WebV2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class ProviderController : Controller
    {
        private ISession Session => HttpContext.Session;
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;
        private readonly IProviderService _providerService;

        public ProviderController(
            ICourseService courseService,
            IVenueService venueService,
            IProviderService providerService)
        {
            if (courseService == null)
            {
                throw new ArgumentNullException(nameof(courseService));
            }

            if (venueService == null)
            {
                throw new ArgumentNullException(nameof(venueService));
            }

            if (providerService == null)
            {
                throw new ArgumentNullException(nameof(providerService));
            }

            _courseService = courseService;
            _venueService = venueService;
            _providerService = providerService;
        }

        [Authorize("Admin")]
        [SelectedProviderNeeded]
        public async Task<IActionResult> AddOrEditProviderType()
        {
            var model = new ProviderTypeAddOrEditViewModel();

            int? UKPRN = Session.GetInt32("UKPRN");

            var providerSearchResult = await _providerService.GetProviderByPRNAsync(UKPRN.ToString());

            if (providerSearchResult.IsSuccess)
            {
                model = new ProviderTypeAddOrEditViewModel();
                model.ProviderType = providerSearchResult.Value.FirstOrDefault()?.ProviderType ?? ProviderType.None;
            }

            return View("UpdateProviderType", model);
        }

        [Authorize("Admin")]
        [HttpPost]
        [SelectedProviderNeeded]
        public async Task<IActionResult> AddOrEditProviderType(
            ProviderTypeAddOrEditViewModel model,
            [FromServices] IProviderInfoCache providerInfoCache,
            [FromServices] ISqlQueryDispatcher sqlQueryDispatcher)
        {
            int? UKPRN = Session.GetInt32("UKPRN");

            var providerSearchResult = await _providerService.GetProviderByPRNAsync(UKPRN.ToString());

            Provider provider;
            if (providerSearchResult.IsSuccess)
            {
                provider = providerSearchResult.Value.FirstOrDefault();

                var oldProviderType = provider.ProviderType;
                if (oldProviderType == ProviderType.FE && model.ProviderType.HasFlag(ProviderType.Apprenticeship))
                {
                    await sqlQueryDispatcher.ExecuteQuery(
                        new EnsureApprenticeshipQAStatusSetForProvider()
                        {
                            ProviderId = provider.id
                        });
                }

                provider.ProviderType = model.ProviderType;

                var result = await _providerService.UpdateProviderDetails(provider);
                if (!result.IsSuccess)
                {
                    throw new Exception(result.Error);
                }

                await providerInfoCache.Remove(provider.id);
            }
            else
            {
                throw new Exception(providerSearchResult.Error);
            }

            return RedirectToAction("ProviderDetails", "Providers", new { providerId = provider.id });
        }

        [Authorize]
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }

        [Authorize]
        [SelectedProviderNeeded]
        public async Task<IActionResult> Courses(
            string level,
            Guid? courseId,
            Guid? courseRunId,
            string notificationTitle,
            string notificationMessage)
        {

            Session.SetString("Option", "Courses");
            int? UKPRN = Session.GetInt32("UKPRN");

            var courseResult = (await _courseService.GetCoursesByLevelForUKPRNAsync(new CourseSearchCriteria(UKPRN))).Value;
            var venueResult = (await _venueService.SearchAsync(new VenueSearchCriteria(UKPRN.ToString(), string.Empty))).Value;
            var allRegions = _courseService.GetRegions().RegionItems;


            var Courses = courseResult.Value.SelectMany(o => o.Value).SelectMany(i => i.Value).ToList();

            var filteredCourses = from Course c in Courses.Where(c => BitmaskHelper.IsSet(c.CourseStatus, RecordStatus.Live)).ToList().OrderBy(x => x.QualificationCourseTitle)
                                  select c;

            var pendingCourses = from Course c in Courses.Where(c => c.CourseStatus== RecordStatus.MigrationPending || c.CourseStatus== RecordStatus.BulkUploadPending)
                                  select c;

            foreach (var course in filteredCourses)
            {
                var filteredCourseRuns = new List<CourseRun>();

                filteredCourseRuns = course.CourseRuns.ToList();
                filteredCourseRuns.RemoveAll(x => x.RecordStatus != RecordStatus.Live);

                course.CourseRuns = filteredCourseRuns;
            }

            var levelFilters = filteredCourses.GroupBy(x => x.NotionalNVQLevelv2).OrderBy(x => x.Key).ToList();

            var levelFiltersForDisplay = new List<QualificationLevelFilterViewModel>();

            var courseViewModels = new List<CourseViewModel>();

            if (string.IsNullOrWhiteSpace(level))
            {
                level = levelFilters.FirstOrDefault()?.Key;
            }
            else
            {
                if (!filteredCourses.Any(x => x.NotionalNVQLevelv2 == level))
                {
                    level = levelFilters.FirstOrDefault()?.Key;
                }
            }

            foreach (var levels in levelFilters)
            {

                var lf = new QualificationLevelFilterViewModel()
                {
                    Facet = levels.ToList().Count().ToString(),
                    IsSelected = level == levels.Key,
                    Value = levels.Key,
                    Name = $"Level {levels.Key}",

                };

                levelFiltersForDisplay.Add(lf);

                foreach (var course in levels)
                {
                    if (course.NotionalNVQLevelv2 == level)
                    {
                        var courseVM = new CourseViewModel()
                        {
                            Id = course.id.ToString(),
                            AwardOrg = course.AwardOrgCode,
                            LearnAimRef = course.LearnAimRef,
                            NotionalNVQLevelv2 = course.NotionalNVQLevelv2,
                            QualificationTitle = course.QualificationCourseTitle,
                            QualificationType = course.QualificationType,
                            Facet = course.CourseRuns.Count().ToString(),
                            CourseRuns = course.CourseRuns.Select(y => new CourseRunViewModel
                            {
                                Id = y.id.ToString(),
                                CourseId = y.ProviderCourseID,
                                AttendancePattern = y.AttendancePattern.ToDescription(),
                                Cost = y.Cost.HasValue ? $"£ {y.Cost.Value:0.00}" : string.Empty,
                                CourseName = y.CourseName,
                                DeliveryMode = y.DeliveryMode.ToDescription(),
                                Duration = y.DurationValue.HasValue
                                        ? $"{y.DurationValue.Value} {y.DurationUnit.ToDescription()}"
                                        : $"0 {y.DurationUnit.ToDescription()}",
                                Venue = y.VenueId.HasValue
                                        ? FormatAddress(GetVenueByIdFrom(venueResult.Value, y.VenueId.Value))
                                        : string.Empty,
                                Region = y.Regions != null
                                        ? FormattedRegionsByIds(allRegions, y.Regions)
                                        : string.Empty,
                                StartDate = y.FlexibleStartDate
                                        ? "Flexible start date"
                                        : y.StartDate?.ToString("dd/MM/yyyy"),
                                StudyMode = y.StudyMode == Services.Models.Courses.StudyMode.Undefined
                                        ? string.Empty
                                        : y.StudyMode.ToDescription(),
                                Url = y.CourseURL
                            })
                                .OrderBy(y => y.CourseName)
                                .ToList()
                        };

                        courseViewModels.Add(courseVM);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(level))
            {
                level = levelFiltersForDisplay.FirstOrDefault()?.Value;
                levelFiltersForDisplay.ForEach(x => { if (x.Value == level) { x.IsSelected = true; } });
            }

            courseViewModels.OrderBy(x => x.QualificationTitle).ToList();

            var notificationCourseName = string.Empty;
            var notificationAnchorTag = string.Empty;

            if (courseId.HasValue && courseId.Value != Guid.Empty)
            {
                notificationCourseName = QualificationTitleByCourseId(courseViewModels, courseId.ToString());

                if (courseRunId.HasValue && courseRunId.Value != Guid.Empty)
                {
                    var courseRuns = courseViewModels.Find(x => x.Id == courseId.Value.ToString())?.CourseRuns;
                    notificationCourseName = CourseNameByCourseRunId(courseRuns, courseRunId.ToString());
                }
            }

            if (!courseRunId.HasValue)
            {
                notificationMessage = string.Empty;
            }

            notificationCourseName = Regex.Replace(notificationCourseName, "<.*?>", String.Empty);
            notificationAnchorTag = courseRunId.HasValue
                ? $"<a id=\"courseeditlink\" class=\"govuk-link\" href=\"#\" data-courseid=\"{courseId}\" data-courserunid=\"{courseRunId}\" >{notificationMessage} {notificationCourseName}</a>"
                : $"<a id=\"courseeditlink\" class=\"govuk-link\" href=\"#\" data-courseid=\"{courseId}\">{notificationMessage} {notificationCourseName}</a>";

            var viewModel = new YourCoursesViewModel
            {
                Heading = $"Level {level}",
                HeadingCaption = "Your courses",
                Courses = courseViewModels ?? new List<CourseViewModel>(),
                LevelFilters = levelFiltersForDisplay,
                NotificationTitle = notificationTitle,
                NotificationMessage = notificationAnchorTag,
                PendingCoursesCount = pendingCourses?.Count()
            };

            return View(viewModel);
        }

        private Venue GetVenueByIdFrom(IEnumerable<Venue> list, Guid id)
        {
            if (list == null) list = new List<Venue>();

            var found = list.ToList().Find(x => x.ID == id.ToString());

            return found;
        }

        private string FormatAddress(Venue venue)
        {
            if (venue == null) return string.Empty;

            return venue.VenueName;
        }

        private string FormattedRegionsByIds(IEnumerable<RegionItemModel> list, IEnumerable<string> ids)
        {
            if (list == null) list = new List<RegionItemModel>();
            if (ids == null) ids = new List<string>();

            var regionNames = (from regionItemModel in list
                               from subRegionItemModel in regionItemModel.SubRegion
                               where ids.Contains(subRegionItemModel.Id)
                               select regionItemModel.RegionName).Distinct().OrderBy(x => x).ToList();

            return string.Join(", ", regionNames);
        }

        private string CourseNameByCourseRunId(IEnumerable<CourseRunViewModel> courseRuns, string id)
        {
            if (courseRuns == null) return string.Empty;
            if (string.IsNullOrWhiteSpace(id)) return string.Empty;

            var found = courseRuns.FirstOrDefault(x => x.Id == id)?.CourseName;

            return found;
        }

        private string QualificationTitleByCourseId(IEnumerable<CourseViewModel> courses, string id)
        {
            if (courses == null) return string.Empty;
            if (string.IsNullOrWhiteSpace(id)) return string.Empty;

            var found = courses.FirstOrDefault(x => x.Id == id)?.QualificationTitle;

            return found;
        }
    }
}
