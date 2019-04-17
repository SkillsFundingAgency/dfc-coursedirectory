using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Helpers;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Regions;
using Dfc.CourseDirectory.Models.Models.Venues;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewModels.YourCourses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Dfc.CourseDirectory.Web.ViewModels.ProviderCourses;

namespace Dfc.CourseDirectory.Web.Controllers.ProviderCourses
{
    public class ProviderCoursesController : Controller
    {
        private readonly ILogger<ProviderCoursesController> _logger;
        private readonly ISession _session;
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;

        public ProviderCoursesController(
            ILogger<ProviderCoursesController> logger,
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

            return venue.VenueName;
        }

        internal string FormattedRegionsByIds(IEnumerable<RegionItemModel> list, IEnumerable<string> ids)
        {
            if (list == null) list = new List<RegionItemModel>();
            if (ids == null) ids = new List<string>();

            var regionNames = (from regionItemModel in list
                               from subRegionItemModel in regionItemModel.SubRegion
                               where ids.Contains(subRegionItemModel.Id)
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
        public async Task<IActionResult> Index()
        {
            _session.SetString("Option", "Courses");
            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var courseResult = (await _courseService.GetCoursesByLevelForUKPRNAsync(new CourseSearchCriteria(UKPRN))).Value;
            var venueResult = (await _venueService.SearchAsync(new VenueSearchCriteria(UKPRN.ToString(), string.Empty))).Value;
            var allRegions = _courseService.GetRegions().RegionItems;

            var allCourses = courseResult.Value.SelectMany(o => o.Value).SelectMany(i => i.Value).ToList();

            var filteredLiveCourses = from Course c in allCourses.Where(c => BitmaskHelper.IsSet(c.CourseStatus, RecordStatus.Live)).ToList().OrderBy(x => x.QualificationCourseTitle) select c;
            var pendingCourses = from Course c in allCourses.Where(c => c.CourseStatus == RecordStatus.MigrationPending || c.CourseStatus == RecordStatus.BulkUloadPending)
                select c;

            var model = new ProviderCoursesViewModel
            {
                PendingCoursesCount = pendingCourses?.Count(),
                ProviderCourseRuns = new List<ProviderCourseRunViewModel>()
            };

            List<ProviderCoursesFilterItemModel> levelFilterItems = new List<ProviderCoursesFilterItemModel>();
            List<ProviderCoursesFilterItemModel> deliveryModelFilterItems = new List<ProviderCoursesFilterItemModel>();
            List<ProviderCoursesFilterItemModel> venueFilterItems = new List<ProviderCoursesFilterItemModel>();
            List<ProviderCoursesFilterItemModel> regionFilterItems = new List<ProviderCoursesFilterItemModel>();
            List<ProviderCoursesFilterItemModel> attendanceModeFilterItems = new List<ProviderCoursesFilterItemModel>();

            foreach (var course in filteredLiveCourses)
            {
                var filteredLiveCourseRuns = new List<CourseRun>();

                filteredLiveCourseRuns = course.CourseRuns.ToList();
                filteredLiveCourseRuns.RemoveAll(x => x.RecordStatus != RecordStatus.Live);

                foreach (var cr in filteredLiveCourseRuns)
                {
                    ProviderCourseRunViewModel courseRunModel = new ProviderCourseRunViewModel()
                    {
                        AwardOrgCode = course.AwardOrgCode,
                        LearnAimRef = course.LearnAimRef,
                        NotionalNVQLevelv2 = course.NotionalNVQLevelv2,
                        QualificationType = course.QualificationType,
                        CourseId = course.id,
                        QualificationCourseTitle = course.QualificationCourseTitle,
                        CourseRunId = cr.id.ToString(),
                        CourseTextId = cr.ProviderCourseID,
                        AttendancePattern = cr.AttendancePattern.ToDescription(),
                        Cost = cr.Cost.HasValue ? $"£ {cr.Cost.Value:0.00}" : string.Empty,
                        CourseName = cr.CourseName,
                        DeliveryMode = cr.DeliveryMode.ToDescription(),
                        Duration = cr.DurationValue.HasValue
                                                        ? $"{cr.DurationValue.Value} {cr.DurationUnit.ToDescription()}"
                                                        : $"0 {cr.DurationUnit.ToDescription()}",
                        Venue = cr.VenueId.HasValue
                                                        ? FormatAddress(GetVenueByIdFrom(venueResult.Value, cr.VenueId.Value))
                                                        : string.Empty,
                        Region = cr.Regions != null
                                                        ? FormattedRegionsByIds(allRegions, cr.Regions)
                                                        : string.Empty,
                        RegionIdList = cr.Regions != null
                            ? FormattedRegionIds(allRegions, cr.Regions)
                            : string.Empty,
                        StartDate = cr.FlexibleStartDate
                                                        ? "Flexible start date"
                                                        : cr.StartDate?.ToString("dd/MM/yyyy"),
                        StudyMode = cr.StudyMode == Models.Models.Courses.StudyMode.Undefined
                                                        ? string.Empty
                                                        : cr.StudyMode.ToDescription(),
                        Url = cr.CourseURL



                    };

                    model.ProviderCourseRuns.Add(courseRunModel);


                }

            }




            int s = 0;
            levelFilterItems = model.ProviderCourseRuns.GroupBy(x => x.NotionalNVQLevelv2).OrderBy(x => x.Key).Select(r => new ProviderCoursesFilterItemModel()
            {
                Id = "level-" + s++.ToString(),
                Value = r.Key,
                Text = "level " + r.Key,
                Name = "level"
            }).ToList();

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

            attendanceModeFilterItems = model.ProviderCourseRuns.Where(x => x.AttendancePattern != AttendancePattern.Undefined.ToString()).GroupBy(x => x.AttendancePattern).OrderBy(x => x.Key).Select(r => new ProviderCoursesFilterItemModel()
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

            model.HasFilters = false;
            model.Levels = levelFilterItems;
            model.DeliveryModes = deliveryModelFilterItems;
            model.Venues = venueFilterItems;
            model.AttendancePattern = attendanceModeFilterItems;
            model.Regions = regionFilterItems;
            return View(model);
        }


        [Authorize]
        public async Task<IActionResult> FilterCourses(ProviderCoursesRequestModel requestModel)
        {
            _session.SetString("Option", "Courses");
            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var courseResult = (await _courseService.GetCoursesByLevelForUKPRNAsync(new CourseSearchCriteria(UKPRN))).Value;
            var venueResult = (await _venueService.SearchAsync(new VenueSearchCriteria(UKPRN.ToString(), string.Empty))).Value;
            var allRegions = _courseService.GetRegions().RegionItems;

            var allCourses = courseResult.Value.SelectMany(o => o.Value).SelectMany(i => i.Value).ToList();

            var filteredLiveCourses = from Course c in allCourses.Where(c => BitmaskHelper.IsSet(c.CourseStatus, RecordStatus.Live)).ToList().OrderBy(x => x.QualificationCourseTitle) select c;

            var model = new ProviderCoursesViewModel();

            model.ProviderCourseRuns = new List<ProviderCourseRunViewModel>();

            List<ProviderCoursesFilterItemModel> levelFilterItems = new List<ProviderCoursesFilterItemModel>();
            List<ProviderCoursesFilterItemModel> deliveryModelFilterItems = new List<ProviderCoursesFilterItemModel>();
            List<ProviderCoursesFilterItemModel> venueFilterItems = new List<ProviderCoursesFilterItemModel>();
            List<ProviderCoursesFilterItemModel> regionFilterItems = new List<ProviderCoursesFilterItemModel>();
            List<ProviderCoursesFilterItemModel> attendanceModeFilterItems = new List<ProviderCoursesFilterItemModel>();

            foreach (var course in filteredLiveCourses)
            {
                var filteredLiveCourseRuns = new List<CourseRun>();

                filteredLiveCourseRuns = course.CourseRuns.ToList();
                filteredLiveCourseRuns.RemoveAll(x => x.RecordStatus != RecordStatus.Live);

                foreach (var cr in filteredLiveCourseRuns)
                {
                    ProviderCourseRunViewModel courseRunModel = new ProviderCourseRunViewModel()
                    {
                        AwardOrgCode = course.AwardOrgCode,
                        LearnAimRef = course.LearnAimRef,
                        NotionalNVQLevelv2 = course.NotionalNVQLevelv2,
                        QualificationType = course.QualificationType,
                        CourseId = course.id,
                        QualificationCourseTitle = course.QualificationCourseTitle,
                        CourseRunId = cr.id.ToString(),
                        CourseTextId = cr.ProviderCourseID,
                        AttendancePattern = cr.AttendancePattern.ToDescription(),
                        Cost = cr.Cost.HasValue ? $"£ {cr.Cost.Value:0.00}" : string.Empty,
                        CourseName = cr.CourseName,
                        DeliveryMode = cr.DeliveryMode.ToDescription(),
                        RegionIdList = cr.Regions != null
                            ? FormattedRegionIds(allRegions, cr.Regions)
                            : string.Empty,
                        Duration = cr.DurationValue.HasValue
                                                        ? $"{cr.DurationValue.Value} {cr.DurationUnit.ToDescription()}"
                                                        : $"0 {cr.DurationUnit.ToDescription()}",
                        Venue = cr.VenueId.HasValue
                                                        ? FormatAddress(GetVenueByIdFrom(venueResult.Value, cr.VenueId.Value))
                                                        : string.Empty,
                        Region = cr.Regions != null
                                                        ? FormattedRegionsByIds(allRegions, cr.Regions)
                                                        : string.Empty,
                        StartDate = cr.FlexibleStartDate
                                                        ? "Flexible start date"
                                                        : cr.StartDate?.ToString("dd/MM/yyyy"),
                        StudyMode = cr.StudyMode == Models.Models.Courses.StudyMode.Undefined
                                                        ? string.Empty
                                                        : cr.StudyMode.ToDescription(),
                        Url = cr.CourseURL



                    };

                    model.ProviderCourseRuns.Add(courseRunModel);


                }

            }

            List<ProviderCourseRunViewModel> aa = new List<ProviderCourseRunViewModel>();
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
              

                List<ProviderCourseRunViewModel> filterResults = new List<ProviderCourseRunViewModel>();
                foreach (var regionFilter in requestModel.RegionFilter)
                {
                    var region = _courseService.GetRegions().RegionItems
                        .Where(x => string.Equals(x.Id, regionFilter, StringComparison.CurrentCultureIgnoreCase))
                        .Select(d => d.RegionName).FirstOrDefault();

                    filterResults.AddRange(model.ProviderCourseRuns.Where(x =>x.Region.Contains(region)));
                }

                model.ProviderCourseRuns = filterResults;
            }

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

            attendanceModeFilterItems = model.ProviderCourseRuns.Where(x => x.AttendancePattern != AttendancePattern.Undefined.ToString()).GroupBy(x => x.AttendancePattern).OrderBy(x => x.Key).Select(r => new ProviderCoursesFilterItemModel()
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