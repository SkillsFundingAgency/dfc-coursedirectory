using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Services.Models.Regions;
using Dfc.CourseDirectory.Web.Extensions;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewModels.ProviderCourses;
using Dfc.CourseDirectory.WebV2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CourseRun = Dfc.CourseDirectory.Core.DataStore.Sql.Models.CourseRun;
using Venue = Dfc.CourseDirectory.Core.DataStore.Sql.Models.Venue;

namespace Dfc.CourseDirectory.Web.Controllers.ProviderCourses
{
    public class ProviderCoursesController : Controller
    {
        private readonly ILogger<ProviderCoursesController> _logger;
        private ISession Session => HttpContext.Session;
        private readonly ICourseService _courseService;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IProviderContextProvider _providerContextProvider;

        public ProviderCoursesController(
            ILogger<ProviderCoursesController> logger,
            ICourseService courseService,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderContextProvider providerContextProvider)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (courseService == null)
            {
                throw new ArgumentNullException(nameof(courseService));
            }

            _logger = logger;
            _courseService = courseService;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerContextProvider = providerContextProvider;
        }


        internal Venue GetVenueByIdFrom(IEnumerable<Venue> list, Guid id) => list.SingleOrDefault(v => v.VenueId == id);

        internal string FormatAddress(Venue venue)
        {
            if (venue == null) return string.Empty;

            return venue.VenueName;
        }

        internal string FormattedRegionsByIds(IEnumerable<RegionItemModel> list, IEnumerable<string> ids)
        {
            if (list == null) list = new List<RegionItemModel>();
            if (ids == null) ids = new List<string>();

            var regionNames = (from regionItemModel in list
                               from subRegionItemModel in regionItemModel.SubRegion
                               where ids.Contains(subRegionItemModel.Id) || ids.Contains(regionItemModel.Id)
                               select regionItemModel.RegionName).Distinct().OrderBy(x => x).ToList();

            return string.Join(", ", regionNames);
        }

        internal string FormattedRegionIds(IEnumerable<RegionItemModel> list, IEnumerable<string> ids)
        {
            if (list == null) list = new List<RegionItemModel>();
            if (ids == null) ids = new List<string>();

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
        public async Task<IActionResult> Index(
            Guid? courseRunId,
            string notificationTitle,
            string notificationMessage)
        {
            Session.SetString("Option", "Courses");
            int? UKPRN = Session.GetInt32("UKPRN");

            var providerId = _providerContextProvider.GetProviderId(withLegacyFallback: true);

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var providerCourses = (await _sqlQueryDispatcher.ExecuteQuery(new GetCoursesForProvider() { ProviderId = providerId }))
                .OrderBy(c => c.LearnAimRefTypeDesc)
                .ThenBy(c => c.LearnAimRef)
                .ToArray();

            var providerVenues = await _sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = providerId });

            var allRegions = _courseService.GetRegions().RegionItems;

            var model = new ProviderCoursesViewModel()
            {
                PendingCoursesCount = 0,
                ProviderCourseRuns = new List<ProviderCourseRunViewModel>()
            };

            List<ProviderCoursesFilterItemModel> levelFilterItems = new List<ProviderCoursesFilterItemModel>();
            List<ProviderCoursesFilterItemModel> deliveryModelFilterItems = new List<ProviderCoursesFilterItemModel>();
            List<ProviderCoursesFilterItemModel> venueFilterItems = new List<ProviderCoursesFilterItemModel>();
            List<ProviderCoursesFilterItemModel> regionFilterItems = new List<ProviderCoursesFilterItemModel>();
            List<ProviderCoursesFilterItemModel> attendanceModeFilterItems = new List<ProviderCoursesFilterItemModel>();

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
                        National = national



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
                    model.ProviderCourseRuns.Add(courseRunModel);


                }

            }

            Session.SetObject("ProviderCourses", model.ProviderCourseRuns);

            int s = 0;
            var textValue = string.Empty;
            var levelFilter = model.ProviderCourseRuns.GroupBy(x => x.NotionalNVQLevelv2).OrderBy(x => x.Key).ToList();
            foreach (var level in levelFilter)
            {
                textValue = string.Empty;

                switch (level.Key.ToLower())
                {
                    case "e":
                        textValue = "Entry level";
                        break;
                    case "x":
                        textValue = "X - Not applicable/unknown";
                        break;
                    case "h":
                        textValue = "Higher";
                        break;
                    case "m":
                        textValue = "Mixed";
                        break;
                    default:
                        textValue = "Level " + level.Key;
                        break;

                }

                ProviderCoursesFilterItemModel itemModel = new ProviderCoursesFilterItemModel()
                {
                    Id = "level-" + s++.ToString(),
                    Value = level.Key,
                    Text = textValue,
                    Name = "level"
                };

                levelFilterItems.Add(itemModel);
            }

            var entryItem = levelFilterItems.Where(x => x.Text.ToLower().Contains("entry")).SingleOrDefault();

            if (entryItem != null)
            {
                levelFilterItems.Remove(entryItem);
                levelFilterItems.Insert(0, entryItem);
            }

            s = 0;
            deliveryModelFilterItems = model.ProviderCourseRuns.GroupBy(x => x.DeliveryMode).OrderBy(x => x.Key).Select(r => new ProviderCoursesFilterItemModel()
            {
                Id = "deliverymode-" + s++.ToString(),
                Value = r.Key,
                Text = r.Key,
                Name = "deliverymode"
            }).ToList();

            s = 0;
            venueFilterItems = model.ProviderCourseRuns.Where(x => !string.IsNullOrEmpty(x.Venue)).GroupBy(x => x.Venue).OrderBy(x => x.Key).Select(r => new ProviderCoursesFilterItemModel()
            {
                Id = "venue-" + s++.ToString(),
                Value = r.Key,
                Text = r.Key,
                Name = "venue"
            }).ToList();

            attendanceModeFilterItems = model.ProviderCourseRuns.Where(x => x.AttendancePattern != string.Empty).GroupBy(x => x.AttendancePattern).OrderBy(x => x.Key).Select(r => new ProviderCoursesFilterItemModel()
            {
                Id = "attendancepattern-" + s++.ToString(),
                Value = r.Key,
                Text = r.Key,
                Name = "attendancepattern"
            }).ToList();

            List<string> allRegionsList = new List<string>();

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
                    Name = "region"
                };

                regionFilterItems.Add(regionFilterItem);
            }

            var notificationCourseName = string.Empty;
            var notificationAnchorTag = string.Empty;

            if (courseRunId.HasValue && courseRunId.Value != Guid.Empty)
            {
                model.CourseRunId = courseRunId.Value.ToString();
                bool courseRunExists = model.ProviderCourseRuns.Any(x => x.CourseRunId == courseRunId.ToString());

                if (courseRunExists == false)
                {
                    model.NotificationTitle = notificationTitle;
                    model.NotificationMessage = notificationMessage;
                }
                else
                {
                    notificationCourseName = Regex.Replace(
                                                model.ProviderCourseRuns.Where(x => x.CourseRunId == courseRunId.Value.ToString()).Select(x => x.CourseName).FirstOrDefault().ToString(), "<.*?>"
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





            model.HasFilters = false;
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
        public IActionResult FilterCourses(ProviderCoursesRequestModel requestModel)
        {

            Session.SetString("Option", "Courses");
            int? UKPRN = Session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var model = new ProviderCoursesViewModel();
            model.ProviderCourseRuns = Session.GetObject<List<ProviderCourseRunViewModel>>("ProviderCourses");

            List<ProviderCoursesFilterItemModel> levelFilterItems = new List<ProviderCoursesFilterItemModel>();
            List<ProviderCoursesFilterItemModel> deliveryModelFilterItems = new List<ProviderCoursesFilterItemModel>();
            List<ProviderCoursesFilterItemModel> venueFilterItems = new List<ProviderCoursesFilterItemModel>();
            List<ProviderCoursesFilterItemModel> regionFilterItems = new List<ProviderCoursesFilterItemModel>();
            List<ProviderCoursesFilterItemModel> attendanceModeFilterItems = new List<ProviderCoursesFilterItemModel>();

            if (!string.IsNullOrEmpty(requestModel.Keyword))
            {
                model.ProviderCourseRuns = model.ProviderCourseRuns
                    .Where(x => x.CourseName.ToLower().Contains(requestModel.Keyword.ToLower())
                                || x.QualificationCourseTitle.ToLower().Contains(requestModel.Keyword.ToLower())
                                || x.LearnAimRef.ToLower().Contains(requestModel.Keyword.ToLower())
                                 || x.AttendancePattern.ToLower().Contains(requestModel.Keyword.ToLower())
                                  || x.DeliveryMode.ToLower().Contains(requestModel.Keyword.ToLower())
                                   || x.Venue.ToLower().Contains(requestModel.Keyword.ToLower())
                                    || x.Region.ToLower().Contains(requestModel.Keyword.ToLower())
                                || (!string.IsNullOrEmpty(x.CourseTextId) && x.CourseTextId.ToLower().Contains(requestModel.Keyword.ToLower()))
                                ).ToList();
            }

            if (requestModel.LevelFilter.Length > 0)
            {
                model.ProviderCourseRuns = model.ProviderCourseRuns.Where(x => requestModel.LevelFilter.Contains(x.NotionalNVQLevelv2)).ToList();
            }

            if (requestModel.DeliveryModeFilter.Length > 0)
            {
                model.ProviderCourseRuns = model.ProviderCourseRuns.Where(x => requestModel.DeliveryModeFilter.Contains(x.DeliveryMode)).ToList();
            }


            if (requestModel.VenueFilter.Length > 0)
            {
                model.ProviderCourseRuns = model.ProviderCourseRuns.Where(x => requestModel.VenueFilter.Contains(x.Venue)).ToList();
            }

            if (requestModel.AttendancePatternFilter.Length > 0)
            {
                model.ProviderCourseRuns = model.ProviderCourseRuns.Where(x => requestModel.AttendancePatternFilter.Contains(x.AttendancePattern)).ToList();
            }


            if (requestModel.RegionFilter.Length > 0)
            {

                List<ProviderCourseRunViewModel> allResults = model.ProviderCourseRuns.ToList();
                List<ProviderCourseRunViewModel> filterResults = new List<ProviderCourseRunViewModel>();
                foreach (var regionFilter in requestModel.RegionFilter)
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

            // _session.SetObject("ProviderCourses", model.ProviderCourseRuns.OrderBy(x=>x.CourseName));


            int s = 0;
            levelFilterItems = model.ProviderCourseRuns.GroupBy(x => x.NotionalNVQLevelv2).OrderBy(x => x.Key).Select(r => new ProviderCoursesFilterItemModel()
            {
                Id = "level-" + s++.ToString(),
                Value = r.Key,
                Text = "Level " + r.Key,
                Name = "level",
                IsSelected = requestModel.LevelFilter.Length > 0 && requestModel.LevelFilter.Contains(r.Key)
            }).ToList();

            s = 0;
            deliveryModelFilterItems = model.ProviderCourseRuns.GroupBy(x => x.DeliveryMode).OrderBy(x => x.Key).Select(r => new ProviderCoursesFilterItemModel()
            {
                Id = "deliverymode-" + s++.ToString(),
                Value = r.Key,
                Text = r.Key,
                Name = "deliverymode",
                IsSelected = requestModel.DeliveryModeFilter.Length > 0 && requestModel.DeliveryModeFilter.Contains(r.Key)
            }).ToList();


            venueFilterItems = model.ProviderCourseRuns.Where(x => !string.IsNullOrEmpty(x.Venue)).GroupBy(x => x.Venue).OrderBy(x => x.Key).Select(r => new ProviderCoursesFilterItemModel()
            {
                Id = "venue-" + s++.ToString(),
                Value = r.Key,
                Text = r.Key,
                Name = "venue",
                IsSelected = requestModel.VenueFilter.Length > 0 && requestModel.VenueFilter.Contains(r.Key)
            }).ToList();

            attendanceModeFilterItems = model.ProviderCourseRuns.Where(x => x.AttendancePattern != string.Empty).GroupBy(x => x.AttendancePattern).OrderBy(x => x.Key).Select(r => new ProviderCoursesFilterItemModel()
            {
                Id = "attendancepattern-" + s++.ToString(),
                Value = r.Key,
                Text = r.Key,
                Name = "attendancepattern",
                IsSelected = requestModel.AttendancePatternFilter.Length > 0 && requestModel.AttendancePatternFilter.Contains(r.Key)
            }).ToList();

            List<string> allRegionsList = new List<string>();
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
                    IsSelected = requestModel.RegionFilter.Length > 0 && requestModel.RegionFilter.Contains(regionId)
                };

                regionFilterItems.Add(regionFilterItem);
            }



            model.HasFilters = levelFilterItems.Any(x => x.IsSelected) || deliveryModelFilterItems.Any(x => x.IsSelected) || venueFilterItems.Any(x => x.IsSelected) || regionFilterItems.Any(x => x.IsSelected) || attendanceModeFilterItems.Any(x => x.IsSelected);
            model.Levels = levelFilterItems;
            model.DeliveryModes = deliveryModelFilterItems;
            model.Venues = venueFilterItems;
            model.Regions = regionFilterItems;
            model.AttendancePattern = attendanceModeFilterItems;

            return ViewComponent(nameof(ViewComponents.ProviderCoursesResults.ProviderCoursesResults), model);
        }
    }
}
