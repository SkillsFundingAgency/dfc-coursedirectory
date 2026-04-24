using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Extensions;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models.Regions;
using Dfc.CourseDirectory.WebV2.Extensions;
using Dfc.CourseDirectory.WebV2.Helpers;
using Dfc.CourseDirectory.WebV2.ViewComponents.GdsPagination;
using Dfc.CourseDirectory.WebV2.ViewComponents.RequestModels;
using Dfc.CourseDirectory.WebV2.ViewModels.ProviderCourses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CourseRun = Dfc.CourseDirectory.Core.DataStore.Sql.Models.CourseRun;
using Venue = Dfc.CourseDirectory.Core.DataStore.Sql.Models.Venue;

namespace Dfc.CourseDirectory.WebV2.Controllers.ProviderCourses
{
    public class ProviderCoursesController : BaseController
    {
        private const int DefaultPageSize = 10;

        private ISession Session => HttpContext.Session;
        private readonly ILogger<ProviderCoursesController> _logger;
        private readonly ICourseService _courseService;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IProviderContextProvider _providerContextProvider;

        public ProviderCoursesController(
            ILogger<ProviderCoursesController> logger,
            ICourseService courseService,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderContextProvider providerContextProvider) : base(sqlQueryDispatcher)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(courseService);

            _logger = logger;
            _courseService = courseService;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerContextProvider = providerContextProvider;
        }

        private static Venue GetVenueByIdFrom(IEnumerable<Venue> list, Guid id) => list.SingleOrDefault(v => v.VenueId == id);

        private static string FormatAddress(Venue venue)
        {
            if (venue == null) return string.Empty;

            return venue.VenueName;
        }

        private static string FormattedRegionsByIds(IEnumerable<RegionItemModel> list, IEnumerable<string> ids)
        {
            list ??= new List<RegionItemModel>();
            ids ??= new List<string>();

            var regionNames = (from regionItemModel in list
                               from subRegionItemModel in regionItemModel.SubRegion
                               where ids.Contains(subRegionItemModel.Id) || ids.Contains(regionItemModel.Id)
                               select regionItemModel.RegionName).Distinct().OrderBy(x => x).ToList();

            return string.Join(", ", regionNames);
        }

        private static string FormattedRegionIds(IEnumerable<RegionItemModel> list, IEnumerable<string> ids)
        {
            list ??= new List<RegionItemModel>();
            ids ??= new List<string>();

            var regionNames = (from regionItemModel in list
                               from subRegionItemModel in regionItemModel.SubRegion
                               where ids.Contains(subRegionItemModel.Id)
                               select regionItemModel.Id).Distinct().OrderBy(x => x).ToList();

            return string.Join(", ", regionNames);
        }

        [Authorize]
        public IActionResult MigratedCourses(string UKPRN)
        {
            Session.SetInt32("UKPRN", Convert.ToInt32(UKPRN));
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(
            int page = 1,
            Guid? courseRunId = null,
            string notificationTitle = null,
            string notificationMessage = null,
            bool nlc = false)
        {
            Session.SetString("Option", "Courses");
            int? UKPRN = Session.GetInt32("UKPRN");

            var providerId = _providerContextProvider.GetProviderId(withLegacyFallback: true);

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var searchState = Session.GetObject<ProviderCourseSearchState>(SessionProviderCoursesSearchState)
                ?? new ProviderCourseSearchState { NonLarsCourse = nlc, PageSize = DefaultPageSize };

            var nonLarsCourse = searchState.NonLarsCourse;
            if (nonLarsCourse)
            {
                Session.SetString(SessionNonLarsCourse, "true");
            }
            else
            {
                Session.SetString(SessionNonLarsCourse, "false");
            }

            var allCourseRuns = Session.GetObject<List<ProviderCourseRunViewModel>>(SessionProviderCourses);
            if (allCourseRuns == null)
            {
                var providerCourses = await GetProviderCourses(nonLarsCourse, providerId);

                var providerVenues = await _sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = providerId });

                var allRegions = _courseService.GetRegions().RegionItems;

                allCourseRuns = new List<ProviderCourseRunViewModel>();

                foreach (var course in providerCourses)
                {
                    var filteredLiveCourseRuns = new List<CourseRun>();

                    filteredLiveCourseRuns = course.CourseRuns.ToList();

                    foreach (var cr in filteredLiveCourseRuns)
                    {
                        var national = cr.DeliveryMode == CourseDeliveryMode.WorkBased & !cr.National.HasValue ||
                                       cr.National.GetValueOrDefault();

                        ProviderCourseRunViewModel courseRunModel = new ProviderCourseRunViewModel()
                        {
                            AwardOrgCode = course.AwardOrgCode,
                            LearnAimRef = course.LearnAimRef,
                            NotionalNVQLevelv2 = course.NotionalNVQLevelv2,
                            QualificationType = course.LearnAimRefTitle,
                            CourseId = course.CourseId,
                            QualificationCourseTitle = course.LearnAimRefTypeDesc,
                            CourseRunId = cr.CourseRunId.ToString(),
                            CourseTextId = cr.ProviderCourseId,
                            AttendancePattern = cr.AttendancePattern.ToDescription(),
                            Cost = cr.Cost.HasValue ? $"£ {cr.Cost.Value:0.00}" : string.Empty,
                            CourseName = cr.CourseName,
                            DeliveryMode = cr.DeliveryMode.ToDescription(),
                            Duration = cr.DurationValue.HasValue
                                                            ? $"{cr.DurationValue.Value} {cr.DurationUnit.ToDescription()}"
                                                            : $"0 {cr.DurationUnit.ToDescription()}",
                            Venue = cr.VenueId.HasValue
                                                            ? FormatAddress(GetVenueByIdFrom(providerVenues, cr.VenueId.Value))
                                                            : string.Empty,
                            StartDate = cr.FlexibleStartDate
                                                            ? "Flexible start date"
                                                            : cr.StartDate?.ToString("dd MMM yyyy"),
                            StudyMode = !cr.StudyMode.HasValue
                                                            ? string.Empty
                                                            : cr.StudyMode.ToDescription(),
                            Url = cr.CourseWebsite,
                            National = national,
                            CourseType = course.CourseType.ToDescription(),
                            SectorId = course.SectorId,
                            EducationLevel = course.EducationLevel.ToDescription(),
                            AwardingBody = course.AwardingBody,
                            IsExpired = course.IsExpired
                        };
                        //If National
                        if (national)
                        {
                            courseRunModel.Region = string.Join(", ", allRegions.Select(x => x.RegionName).ToList());
                            courseRunModel.RegionIdList = string.Join(", ", allRegions.Select(x => x.Id).ToList());
                        }
                        else
                        {
                            courseRunModel.Region = cr.SubRegionIds?.Count() > 0 ? FormattedRegionsByIds(allRegions, cr.SubRegionIds) : string.Empty;
                            courseRunModel.RegionIdList = cr.SubRegionIds?.Count() > 0 ? FormattedRegionIds(allRegions, cr.SubRegionIds) : string.Empty;
                        }
                        allCourseRuns.Add(courseRunModel);
                    }
                }

                Session.SetObject(SessionProviderCourses, allCourseRuns);
            }

            var model = new ProviderCoursesViewModel()
            {
                PendingCoursesCount = 0,
                TotalCoursesCount = allCourseRuns.Count,
                ProviderCourseRuns = allCourseRuns.ToList(),
                NonLarsCourse = nonLarsCourse
            };

            var keywordSearch = searchState.Keyword?.Trim()?.ToLower() ?? string.Empty;
            var keywordTooShort = keywordSearch.Length > 0 && keywordSearch.Length < 3;
            if (keywordTooShort)
            {
                ModelState.AddModelError(nameof(searchState.Keyword), "Enter a minimum of 3 characters");
                keywordSearch = string.Empty;
            }

            if (keywordSearch.Length > 0)
            {
                model.ProviderCourseRuns = model.ProviderCourseRuns
                    .Where(x => x.CourseName.ToLower().Contains(keywordSearch)
                                || (!string.IsNullOrWhiteSpace(x.QualificationCourseTitle) && x.QualificationCourseTitle.ToLower().Contains(keywordSearch))
                                || (!string.IsNullOrWhiteSpace(x.LearnAimRef) && x.LearnAimRef.ToLower().Contains(keywordSearch))
                                 || x.AttendancePattern.ToLower().Contains(keywordSearch)
                                  || x.DeliveryMode.ToLower().Contains(keywordSearch)
                                   || x.Venue.ToLower().Contains(keywordSearch)
                                    || x.Region.ToLower().Contains(keywordSearch)
                                || (!string.IsNullOrEmpty(x.CourseTextId) && x.CourseTextId.ToLower().Contains(keywordSearch))
                                ).ToList();
            }

            if (searchState.LevelFilter.Length > 0)
            {
                model.ProviderCourseRuns = model.ProviderCourseRuns.Where(x => searchState.LevelFilter.Contains(x.NotionalNVQLevelv2)).ToList();
            }

            if (searchState.DeliveryModeFilter.Length > 0)
            {
                model.ProviderCourseRuns = model.ProviderCourseRuns.Where(x => searchState.DeliveryModeFilter.Contains(x.DeliveryMode)).ToList();
            }

            if (searchState.VenueFilter.Length > 0)
            {
                model.ProviderCourseRuns = model.ProviderCourseRuns.Where(x => searchState.VenueFilter.Contains(x.Venue)).ToList();
            }

            if (searchState.AttendancePatternFilter.Length > 0)
            {
                model.ProviderCourseRuns = model.ProviderCourseRuns.Where(x => searchState.AttendancePatternFilter.Contains(x.AttendancePattern)).ToList();
            }

            if (searchState.RegionFilter.Length > 0)
            {
                List<ProviderCourseRunViewModel> allResults = model.ProviderCourseRuns.ToList();
                List<ProviderCourseRunViewModel> filterResults = new();
                foreach (var regionFilter in searchState.RegionFilter)
                {
                    var region = _courseService.GetRegions().RegionItems
                        .Where(x => string.Equals(x.Id, regionFilter, StringComparison.CurrentCultureIgnoreCase))
                        .Select(d => d.RegionName).FirstOrDefault();

                    var results = allResults.Where(x => x.Region.Contains(region));

                    filterResults.AddRange(results);

                    allResults.RemoveAll(x => x.Region.Contains(region));
                }

                model.ProviderCourseRuns = filterResults;
            }

            model.ProviderCourseRuns = model.ProviderCourseRuns.OrderBy(x => x.CourseName).ToList();

            List<ProviderCoursesFilterItemModel> levelFilterItems = new();
            List<ProviderCoursesFilterItemModel> deliveryModelFilterItems = new();
            List<ProviderCoursesFilterItemModel> venueFilterItems = new();
            List<ProviderCoursesFilterItemModel> regionFilterItems = new();
            List<ProviderCoursesFilterItemModel> attendanceModeFilterItems = new();

            int s = 0;

            if (!nonLarsCourse)
            {
                var textValue = string.Empty;
                var levelFilter = model.ProviderCourseRuns.GroupBy(x => x.NotionalNVQLevelv2).OrderBy(x => x.Key).ToList();
                foreach (var level in levelFilter)
                {
                    textValue = string.Empty;
                    string levelKey = string.Empty;
                    if (level.Key != null)
                        levelKey = level.Key;

                    textValue = levelKey.ToLower() switch
                    {
                        "e" => "Entry level",
                        "x" => "X - Not applicable/unknown",
                        "h" => "Higher",
                        "m" => "Mixed",
                        _ => "Level " + levelKey,
                    };

                    ProviderCoursesFilterItemModel itemModel = new()
                    {
                        Id = "level-" + s++.ToString(),
                        Value = levelKey,
                        Text = textValue,
                        Name = "level",
                        IsSelected = searchState.LevelFilter.Length > 0 && searchState.LevelFilter.Contains(levelKey)
                    };

                    levelFilterItems.Add(itemModel);
                }

                var entryItem = levelFilterItems.Where(x => x.Text.ToLower().Contains("entry")).SingleOrDefault();

                if (entryItem != null)
                {
                    levelFilterItems.Remove(entryItem);
                    levelFilterItems.Insert(0, entryItem);
                }
            }

            s = 0;
            deliveryModelFilterItems = model.ProviderCourseRuns.GroupBy(x => x.DeliveryMode).OrderBy(x => x.Key).Select(r => new ProviderCoursesFilterItemModel()
            {
                Id = "deliverymode-" + s++.ToString(),
                Value = r.Key,
                Text = r.Key,
                Name = "deliverymode",
                IsSelected = searchState.DeliveryModeFilter.Length > 0 && searchState.DeliveryModeFilter.Contains(r.Key)
            }).ToList();

            s = 0;
            venueFilterItems = model.ProviderCourseRuns.Where(x => !string.IsNullOrEmpty(x.Venue)).GroupBy(x => x.Venue).OrderBy(x => x.Key).Select(r => new ProviderCoursesFilterItemModel()
            {
                Id = "venue-" + s++.ToString(),
                Value = r.Key,
                Text = r.Key,
                Name = "venue",
                IsSelected = searchState.VenueFilter.Length > 0 && searchState.VenueFilter.Contains(r.Key)
            }).ToList();

            attendanceModeFilterItems = model.ProviderCourseRuns.Where(x => x.AttendancePattern != string.Empty).GroupBy(x => x.AttendancePattern).OrderBy(x => x.Key).Select(r => new ProviderCoursesFilterItemModel()
            {
                Id = "attendancepattern-" + s++.ToString(),
                Value = r.Key,
                Text = r.Key,
                Name = "attendancepattern",
                IsSelected = searchState.AttendancePatternFilter.Length > 0 && searchState.AttendancePatternFilter.Contains(r.Key)
            }).ToList();

            List<string> allRegionsList = new();

            var regionsForCourse = model.ProviderCourseRuns.GroupBy(x => x.Region).Where(x => !string.IsNullOrEmpty(x.Key)).OrderBy(x => x.Key).ToList();
            foreach (var regions in regionsForCourse)
            {
                var regionsList = regions.Key.Split(",");
                foreach (var region in regionsList)
                {
                    allRegionsList.Add(region.Trim());
                }
            }

            allRegionsList = allRegionsList.Distinct().ToList();

            s = 0;
            foreach (var regionValue in allRegionsList)
            {
                var regionId = _courseService.GetRegions().RegionItems
                    .Where(x => string.Equals(x.RegionName, regionValue, StringComparison.CurrentCultureIgnoreCase))
                    .Select(d => d.Id).FirstOrDefault();

                var regionFilterItem = new ProviderCoursesFilterItemModel
                {
                    Value = regionId,
                    Text = regionValue,
                    Id = "region-" + s++,
                    Name = "region",
                    IsSelected = searchState.RegionFilter.Length > 0 && searchState.RegionFilter.Contains(regionId)
                };

                regionFilterItems.Add(regionFilterItem);
            }

            var (pagedRuns, pagination) = GdsPaginationModel.Paginate(model.ProviderCourseRuns, page, DefaultPageSize);
            model.ProviderCourseRuns = pagedRuns.ToList();
            model.Keyword = searchState.Keyword;
            model.Pagination = pagination;

            var notificationCourseName = string.Empty;
            var notificationAnchorTag = string.Empty;

            if (courseRunId.HasValue && courseRunId.Value != Guid.Empty)
            {
                model.CourseRunId = courseRunId.Value.ToString();
                bool courseRunExists = allCourseRuns.Any(x => x.CourseRunId == courseRunId.ToString());

                if (courseRunExists == false)
                {
                    model.NotificationTitle = notificationTitle;
                    model.NotificationMessage = notificationMessage;
                }
                else
                {
                    notificationCourseName = Regex.Replace(
                                                allCourseRuns.Where(x => x.CourseRunId == courseRunId.Value.ToString()).Select(x => x.CourseName).FirstOrDefault().ToString(), "<.*?>"
                                                , String.Empty);

                    notificationAnchorTag = courseRunId.HasValue
                      ? $"<a id=\"courseeditlink\" class=\"govuk-link\" href=\"#\" data-courserunid=\"{courseRunId}\" >{notificationMessage} {notificationCourseName}</a>"
                      : $"<a id=\"courseeditlink\" class=\"govuk-link\" href=\"#\">{notificationMessage} {notificationCourseName}</a>";
                    model.NotificationTitle = notificationTitle;
                    model.NotificationMessage = notificationAnchorTag;
                }
            }
            else
            {
                notificationTitle = string.Empty;
                notificationAnchorTag = string.Empty;
            }

            model.HasFilters = 
                levelFilterItems.Any(x => x.IsSelected) || 
                deliveryModelFilterItems.Any(x => x.IsSelected) || 
                venueFilterItems.Any(x => x.IsSelected) || 
                regionFilterItems.Any(x => x.IsSelected) || 
                attendanceModeFilterItems.Any(x => x.IsSelected) || 
                !string.IsNullOrWhiteSpace(searchState.Keyword);
            model.Levels = levelFilterItems;
            model.DeliveryModes = deliveryModelFilterItems;
            model.Venues = venueFilterItems;
            model.AttendancePattern = attendanceModeFilterItems;
            model.Regions = regionFilterItems;

            //Setup backlink to go to the dashboard
            ViewBag.BackLinkController = "Home";
            ViewBag.BackLinkAction = "Index";

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(ProviderCourseSearchState searchState)
        {
            Session.SetString("Option", "Courses");
            int? UKPRN = Session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

            searchState.PageSize = DefaultPageSize;

            var existing = Session.GetObject<ProviderCourseSearchState>(SessionProviderCoursesSearchState);
            if (existing != null && existing.NonLarsCourse != searchState.NonLarsCourse)
                Session.Remove(SessionProviderCourses);

            Session.SetObject(SessionProviderCoursesSearchState, searchState);

            return RedirectToAction(nameof(Index), searchState.NonLarsCourse ? new { nlc = "true" } : null);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ClearFilters(bool nlc = false)
        {
            Session.Remove(SessionProviderCoursesSearchState);
            return RedirectToAction(nameof(Index), nlc ? new { nlc = "true" } : null);
        }

        private async Task<Core.DataStore.Sql.Models.Course[]> GetProviderCourses(bool nonLarsCourse, Guid providerId)
        {
            Core.DataStore.Sql.Models.Course[] providerCourses;
            if (nonLarsCourse)
            {
                providerCourses = (await _sqlQueryDispatcher.ExecuteQuery(new GetNonLarsCoursesForProvider() { ProviderId = providerId }))
                .ToArray();

                return providerCourses;
            }

            providerCourses = (await _sqlQueryDispatcher.ExecuteQuery(new GetCoursesForProvider() { ProviderId = providerId }))
                    .OrderBy(c => c.LearnAimRefTypeDesc)
                    .ThenBy(c => c.LearnAimRef)
                    .ToArray();

            return providerCourses;
        }
    }
}
