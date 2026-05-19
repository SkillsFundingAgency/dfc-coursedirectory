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
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerContextProvider = providerContextProvider;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(
            int page = 1,
            bool nlc = false,
            Guid? courseRunId = null,
            string notificationTitle = null,
            string notificationMessage = null,
            string nce = null)
        {
            Session.SetString("Option", "Courses");
            int? UKPRN = Session.GetInt32("UKPRN");
            var providerId = _providerContextProvider.GetProviderId(withLegacyFallback: true);

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var requestNonce = nce;

            var searchState = Session.GetObject<ProviderCourseSearchState>(SessionProviderCoursesSearchState);
            var storedNonce = Session.GetString(SessionProviderCoursesNonce);

            var isSearchStateSafe = searchState is not null && searchState.NonLarsCourse == nlc;
            var isNonceSafe = !string.IsNullOrEmpty(requestNonce) && requestNonce == storedNonce;

            if (!isSearchStateSafe || !isNonceSafe)
            {
                Session.Remove(SessionProviderCourses);
                Session.Remove(SessionProviderCoursesNonce);
                Session.Remove(SessionProviderCoursesSearchState);
                searchState = null;
            }

            if (searchState is null) 
            {
                searchState = new ProviderCourseSearchState { NonLarsCourse = nlc };
                Session.SetObject(SessionProviderCoursesSearchState, searchState);
            }

            Session.SetString(SessionNonLarsCourse, searchState.NonLarsCourse ? "true" : "false");

            var allCourseRuns = Session.GetObject<List<ProviderCourseRunViewModel>>(SessionProviderCourses);
            if (allCourseRuns == null)
            {
                var courses = await GetProviderCourses(searchState.NonLarsCourse, providerId);
                var venues = await _sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider { ProviderId = providerId });
                var allRegions = _courseService.GetRegions().RegionItems;
                allCourseRuns = BuildCourseRunViewModels(courses, venues, allRegions);
                Session.SetObject(SessionProviderCourses, allCourseRuns);
                requestNonce = Guid.NewGuid().ToString("N");
                Session.SetString(SessionProviderCoursesNonce, requestNonce);
            }

            var keywordSearch = searchState.Keyword?.Trim()?.ToLower() ?? string.Empty;
            var keywordTooShort = keywordSearch.Length > 0 && keywordSearch.Length < 3;
            if (keywordTooShort)
            {
                keywordSearch = string.Empty;
                ModelState.AddModelError(nameof(searchState.Keyword), "Enter a minimum of 3 characters");
            }

            var keywordTooLong = keywordSearch.Length > 50;
            if (keywordTooLong)
            {
                keywordSearch = string.Empty;
                ModelState.AddModelError(nameof(searchState.Keyword), "Enter a maximum of 50 characters");
            }

            var allRegionItems = _courseService.GetRegions().RegionItems;
            var model = BuildViewModel(allCourseRuns, searchState, page, keywordSearch, allRegionItems);
            model.Nonce = requestNonce;

            if (courseRunId.HasValue && courseRunId.Value != Guid.Empty)
            {
                model.CourseRunId = courseRunId.Value.ToString();
                var courseRunExists = allCourseRuns.Any(x => x.CourseRunId == courseRunId.ToString());

                if (!courseRunExists)
                {
                    model.NotificationTitle = notificationTitle;
                    model.NotificationMessage = notificationMessage;
                }
                else
                {
                    var notificationCourseName = Regex.Replace(
                        allCourseRuns.FirstOrDefault(x => x.CourseRunId == courseRunId.Value.ToString())?.CourseName ?? string.Empty,
                        "<.*?>", string.Empty);

                    model.NotificationTitle = notificationTitle;

                    model.NotificationMessage = courseRunId.HasValue 
                        ? $"<a id=\"courseeditlink\" class=\"govuk-link\" href=\"#\" data-courserunid=\"{courseRunId}\" >{notificationMessage} {notificationCourseName}</a>"
                      : $"<a id=\"courseeditlink\" class=\"govuk-link\" href=\"#\">{notificationMessage} {notificationCourseName}</a>";
                }
            }

            ViewBag.BackLinkController = "Home";
            ViewBag.BackLinkAction = "Index";

            return View("Index", model);
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

            var existing = Session.GetObject<ProviderCourseSearchState>(SessionProviderCoursesSearchState);
            if (existing != null && existing.NonLarsCourse != searchState.NonLarsCourse)
                Session.Remove(SessionProviderCoursesSearchState);

            Session.SetObject(SessionProviderCoursesSearchState, searchState);

            var sessionNonce = Session.GetString(SessionProviderCoursesNonce);
            return RedirectToIndex(searchState.NonLarsCourse, sessionNonce);
        }


        [Authorize]
        [HttpGet]
        public IActionResult ClearFilters(bool nlc = false)
        {
            Session.Remove(SessionProviderCoursesSearchState);
            var sessionNonce = Session.GetString(SessionProviderCoursesNonce);
            return RedirectToIndex(nlc, sessionNonce);
        }

        private RedirectToActionResult RedirectToIndex(bool nlc, string nonce)
        {
            return RedirectToAction(nameof(Index), nlc
                ? new { nlc = "true", nce = nonce }
                : new { nce = nonce });
        }


        private static ViewModels.ProviderCourses.ProviderCoursesViewModel BuildViewModel(
            List<ProviderCourseRunViewModel> allCourseRuns,
            ProviderCourseSearchState searchState,
            int page,
            string keywordSearch,
            IEnumerable<RegionItemModel> allRegions)
        {
            var filtered = ApplyFilters(allCourseRuns, searchState, allRegions, keywordSearch);
            
            var (hasFilters, levels, deliveryModes, venues, attendancePattern, regions) =
                BuildFilterOptions(filtered, searchState, searchState.NonLarsCourse, keywordSearch, allRegions);


            var (paginatedProviderCourses, pagination) = GdsPaginationModel.Paginate(filtered, page, DefaultPageSize);

            return new ViewModels.ProviderCourses.ProviderCoursesViewModel
            {
                Keyword = searchState.Keyword,
                NonLarsCourse = searchState.NonLarsCourse,
                ProviderCourseRuns = paginatedProviderCourses.ToList(),
                HasFilters = hasFilters,
                Levels = levels,
                DeliveryModes = deliveryModes,
                Venues = venues,
                AttendancePattern = attendancePattern,
                Regions = regions,
                Pagination = pagination,
                TotalCoursesCount = allCourseRuns.Count
            };
        }

        private static List<ProviderCourseRunViewModel> ApplyFilters(
            List<ProviderCourseRunViewModel> courseRuns,
            ProviderCourseSearchState searchState,
            IEnumerable<RegionItemModel> allRegions,
            string keywordSearch)
        {
            var result = courseRuns.AsEnumerable();
            
            if (!string.IsNullOrWhiteSpace(keywordSearch))
            {
                result = result.Where(x =>
                    x.CourseName.ToLower().Contains(keywordSearch)
                    || (!string.IsNullOrWhiteSpace(x.QualificationCourseTitle) && x.QualificationCourseTitle.ToLower().Contains(keywordSearch))
                    || (!string.IsNullOrWhiteSpace(x.LearnAimRef) && x.LearnAimRef.ToLower().Contains(keywordSearch))
                    || x.AttendancePattern.ToLower().Contains(keywordSearch)
                    || x.DeliveryMode.ToDescription().ToLower().Contains(keywordSearch)
                    || x.Venue.ToLower().Contains(keywordSearch)
                    || x.Region.ToLower().Contains(keywordSearch)
                    || (!string.IsNullOrEmpty(x.CourseTextId) && x.CourseTextId.ToLower().Contains(keywordSearch)));
            }

            if (searchState.LevelFilter?.Length > 0)
                result = result.Where(x => searchState.LevelFilter.Contains(x.NotionalNVQLevelv2));

            if (searchState.DeliveryModeFilter?.Length > 0)
                result = result.Where(x => searchState.DeliveryModeFilter.Contains(x.DeliveryMode.ToDescription()));

            if (searchState.VenueFilter?.Length > 0)
                result = result.Where(x => searchState.VenueFilter.Contains(x.Venue));

            if (searchState.AttendancePatternFilter?.Length > 0)
                result = result.Where(x => searchState.AttendancePatternFilter.Contains(x.AttendancePattern));

            if (searchState.RegionFilter?.Length > 0)
            {
                var regionNames = searchState.RegionFilter
                    .Select(id => allRegions
                        .Where(regionItemModel => string.Equals(regionItemModel.Id, id, StringComparison.OrdinalIgnoreCase))
                        .Select(regionItemModel => regionItemModel.RegionName)
                        .FirstOrDefault())
                    .Where(n => n is not null)
                    .ToList();

                result = result.Where(x => regionNames.Any(regionName => x.Region.Contains(regionName)));
            }

            return result.OrderBy(x => x.CourseName).ToList();
        }

        private static (bool hasFilters,
            List<ProviderCoursesFilterItemModel> levels,
            List<ProviderCoursesFilterItemModel> deliveryModes,
            List<ProviderCoursesFilterItemModel> venues,
            List<ProviderCoursesFilterItemModel> attendancePattern,
            List<ProviderCoursesFilterItemModel> regions) BuildFilterOptions(
            List<ProviderCourseRunViewModel> filtered,
            ProviderCourseSearchState searchState,
            bool nonLarsCourse,
            string keywordSearch,
            IEnumerable<RegionItemModel> allRegions)
        {
            int s = 0;

            var levels = new List<ProviderCoursesFilterItemModel>();
            if (!nonLarsCourse)
            {
                levels = filtered.GroupBy(x => x.NotionalNVQLevelv2).OrderBy(x => x.Key)
                    .Select(r => new ProviderCoursesFilterItemModel
                    {
                        Id = "level-" + s++,
                        Value = r.Key,
                        Text = MapLevelText(r.Key),
                        Name = "level",
                        IsSelected = searchState.LevelFilter?.Contains(r.Key) == true
                    }).ToList();

                var entryItem = levels.SingleOrDefault(x => x.Text.ToLower().Contains("entry"));
                if (entryItem != null) { levels.Remove(entryItem); levels.Insert(0, entryItem); }
            }

            s = 0;
            var deliveryModes = filtered.GroupBy(x => x.DeliveryMode).OrderBy(x => x.Key)
                .Select(r => new ProviderCoursesFilterItemModel
                {
                    Id = "deliverymode-" + s++,
                    Value = r.Key.ToDescription(),
                    Text = r.Key.ToDescription(),
                    Name = "deliverymode",
                    IsSelected = searchState.DeliveryModeFilter?.Contains(r.Key.ToDescription()) == true
                }).ToList();

            var venues = filtered.Where(x => !string.IsNullOrEmpty(x.Venue)).GroupBy(x => x.Venue).OrderBy(x => x.Key)
                .Select(r => new ProviderCoursesFilterItemModel
                {
                    Id = "venue-" + s++,
                    Value = r.Key,
                    Text = r.Key,
                    Name = "venue",
                    IsSelected = searchState.VenueFilter?.Contains(r.Key) == true
                }).ToList();

            var attendancePattern = filtered.Where(x => !string.IsNullOrEmpty(x.AttendancePattern)).GroupBy(x => x.AttendancePattern).OrderBy(x => x.Key)
                .Select(r => new ProviderCoursesFilterItemModel
                {
                    Id = "attendancepattern-" + s++,
                    Value = r.Key,
                    Text = r.Key,
                    Name = "attendancepattern",
                    IsSelected = searchState.AttendancePatternFilter?.Contains(r.Key) == true
                }).ToList();

            var allRegionsList = new List<string>();
            foreach (var group in filtered.GroupBy(x => x.Region).Where(x => !string.IsNullOrEmpty(x.Key)).OrderBy(x => x.Key))
                foreach (var region in group.Key.Split(","))
                    allRegionsList.Add(region.Trim());

            allRegionsList = allRegionsList.Distinct().OrderBy(x => x).ToList();

            s = 0;
            var regions = allRegionsList.Select(regionValue =>
            {
                var regionId = allRegions
                    .Where(x => string.Equals(x.RegionName, regionValue, StringComparison.OrdinalIgnoreCase))
                    .Select(d => d.Id)
                    .FirstOrDefault();

                return new ProviderCoursesFilterItemModel
                {
                    Value = regionId,
                    Text = regionValue,
                    Id = "region-" + s++,
                    Name = "region",
                    IsSelected = searchState.RegionFilter?.Contains(regionId) == true
                };
            }).ToList();

            var hasFilters = levels.Any(x => x.IsSelected)
                || deliveryModes.Any(x => x.IsSelected)
                || venues.Any(x => x.IsSelected)
                || attendancePattern.Any(x => x.IsSelected)
                || regions.Any(x => x.IsSelected)
                || !string.IsNullOrWhiteSpace(keywordSearch);

            return (hasFilters, levels, deliveryModes, venues, attendancePattern, regions);
        }

        private static List<ProviderCourseRunViewModel> BuildCourseRunViewModels(
            Core.DataStore.Sql.Models.Course[] courses,
            IEnumerable<Venue> venues,
            IEnumerable<RegionItemModel> allRegions)
        {
            var result = new List<ProviderCourseRunViewModel>();
            var venueList = venues.ToList();
            var regionList = allRegions.ToList();

            foreach (var course in courses)
            {
                foreach (var cr in course.CourseRuns)
                {
                    var national = cr.DeliveryMode == CourseDeliveryMode.WorkBased && !cr.National.HasValue
                                   || cr.National.GetValueOrDefault();

                    var courseRun = new ProviderCourseRunViewModel
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
                        DeliveryMode = cr.DeliveryMode,
                        Duration = cr.DurationValue.HasValue
                            ? $"{cr.DurationValue.Value} {cr.DurationUnit.ToDescription()}"
                            : $"0 {cr.DurationUnit.ToDescription()}",
                        Venue = cr.VenueId.HasValue
                            ? venueList.SingleOrDefault(v => v.VenueId == cr.VenueId.Value)?.VenueName ?? string.Empty
                            : string.Empty,
                        StartDate = cr.FlexibleStartDate
                            ? "Flexible start date"
                            : cr.StartDate?.ToString("dd MMM yyyy"),
                        StudyMode = cr.StudyMode.HasValue ? cr.StudyMode.ToDescription() : string.Empty,
                        Url = cr.CourseWebsite,
                        National = national,
                        CourseType = course.CourseType.ToDescription(),
                        SectorId = course.SectorId,
                        EducationLevel = course.EducationLevel.ToDescription(),
                        AwardingBody = course.AwardingBody,
                        IsExpired = course.IsExpired
                    };

                    if (national)
                    {
                        courseRun.Region = string.Join(", ", regionList.Select(x => x.RegionName));
                        courseRun.RegionIdList = string.Join(", ", regionList.Select(x => x.Id));
                    }
                    else
                    {
                        courseRun.Region = cr.SubRegionIds?.Any() == true
                            ? FormattedRegionsByIds(regionList, cr.SubRegionIds)
                            : string.Empty;
                        courseRun.RegionIdList = cr.SubRegionIds?.Any() == true
                            ? FormattedRegionIds(regionList, cr.SubRegionIds)
                            : string.Empty;
                    }

                    result.Add(courseRun);
                }
            }

            return result;
        }

        private static string MapLevelText(string levelKey)
        {
            return (levelKey?.ToLower()) switch
            {
                "e" => "Entry level",
                "x" => "X - Not applicable/unknown",
                "h" => "Higher",
                "m" => "Mixed",
                _ => "Level " + levelKey,
            };
        }

        private static string FormattedRegionsByIds(IEnumerable<RegionItemModel> list, IEnumerable<string> ids)
        {
            list ??= new List<RegionItemModel>();
            ids ??= new List<string>();

            var regionNames = (from regionItemModel in list
                               from subRegionItemModel in regionItemModel.SubRegion
                               where ids.Contains(subRegionItemModel.Id) || ids.Contains(regionItemModel.Id)
                               select regionItemModel.RegionName).Distinct().OrderBy(x => x);
            return string.Join(", ", regionNames);
        }

        private static string FormattedRegionIds(IEnumerable<RegionItemModel> list, IEnumerable<string> ids)
        {
            list ??= new List<RegionItemModel>();
            ids ??= new List<string>();

            var regionIds = (from regionItemModel in list
                             from subRegionItemModel in regionItemModel.SubRegion
                             where ids.Contains(subRegionItemModel.Id)
                             select regionItemModel.Id).Distinct().OrderBy(x => x);
            return string.Join(", ", regionIds);
        }

        private async Task<Core.DataStore.Sql.Models.Course[]> GetProviderCourses(bool nonLarsCourse, Guid providerId)
        {
            if (nonLarsCourse)
            {
                return (await _sqlQueryDispatcher.ExecuteQuery(
                    new GetNonLarsCoursesForProvider { ProviderId = providerId })).ToArray();
            }

            return (await _sqlQueryDispatcher.ExecuteQuery(new GetCoursesForProvider { ProviderId = providerId }))
                .OrderBy(c => c.LearnAimRefTypeDesc)
                .ThenBy(c => c.LearnAimRef)
                .ToArray();
        }
    }
}
