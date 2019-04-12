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

                foreach(var cr in filteredLiveCourseRuns)
                {
                    ProviderCourseRunViewModel courseRunModel = new ProviderCourseRunViewModel()
                    {
                        AwardOrgCode = course.AwardOrgCode,
                        LearnAimRef = course.LearnAimRef,
                        NotionalNVQLevelv2 = course.NotionalNVQLevelv2,
                        QualificationType =  course.QualificationType,
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
            levelFilterItems = model.ProviderCourseRuns.GroupBy(x => x.NotionalNVQLevelv2).OrderBy(x => x.Key).Select(r=>new ProviderCoursesFilterItemModel()
            {
                Id = "level-"+ s++.ToString(),
                Value = r.Key,
                Text = "level "+ r.Key,
                Name = "level"
            }).ToList();

            s = 0;
            deliveryModelFilterItems = model.ProviderCourseRuns.GroupBy(x => x.DeliveryMode).OrderBy(x => x.Key).Select(r => new ProviderCoursesFilterItemModel()
            {
                Id = "deliverymode-" + s++.ToString(),
                 Value = r.Key,
                Text = r.Key,
                Name ="deliverymode"
            }).ToList();


            //levelFilterItems.Add(new ProviderCoursesFilterItemModel()
            //{
            //    Text = "Level " + course.NotionalNVQLevelv2,
            //    Value = course.NotionalNVQLevelv2

            //});

            model.Levels = levelFilterItems;
            model.DeliveryModes = deliveryModelFilterItems;

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
                model.ProviderCourseRuns =model.ProviderCourseRuns.Where(x => requestModel.LevelFilter.Contains(x.NotionalNVQLevelv2)).ToList();
            }

            if (requestModel.DeliveryModeFilter.Length > 0)
            {
                model.ProviderCourseRuns = model.ProviderCourseRuns.Where(x => requestModel.DeliveryModeFilter.Contains(x.DeliveryMode)).ToList();
            }

            int s = 0;
            levelFilterItems = model.ProviderCourseRuns.GroupBy(x => x.NotionalNVQLevelv2).OrderBy(x => x.Key).Select(r => new ProviderCoursesFilterItemModel()
            {
                Id = "level-" + s++.ToString(),
                Value = r.Key,
                Text = "level " + r.Key,
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


            //levelFilterItems.Add(new ProviderCoursesFilterItemModel()
            //{
            //    Text = "Level " + course.NotionalNVQLevelv2,
            //    Value = course.NotionalNVQLevelv2

            //});

            model.HasFilters = levelFilterItems.Any(x => x.IsSelected);
            model.Levels = levelFilterItems;
            model.DeliveryModes = deliveryModelFilterItems;

            return ViewComponent(nameof(ViewComponents.ProviderCoursesResults.ProviderCoursesResults), model);
        }




        //[Authorize]
        //public async Task<IActionResult> Courses(
        //    string level,
        //    Guid? courseId,
        //    Guid? courseRunId,
        //    string notificationTitle,
        //    string notificationMessage)
        //{

        //    _session.SetString("Option", "Courses");
        //    int? UKPRN = _session.GetInt32("UKPRN");

        //    if (!UKPRN.HasValue)
        //    {
        //        return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
        //    }

        //    var courseResult = (await _courseService.GetCoursesByLevelForUKPRNAsync(new CourseSearchCriteria(UKPRN))).Value;
        //    var venueResult = (await _venueService.SearchAsync(new VenueSearchCriteria(UKPRN.ToString(), string.Empty))).Value;
        //    var allRegions = _courseService.GetRegions().RegionItems;


        //    var Courses = courseResult.Value.SelectMany(o => o.Value).SelectMany(i => i.Value).ToList();

        //    var filteredCourses = from Course c in Courses.Where(c => BitmaskHelper.IsSet(c.CourseStatus, RecordStatus.Live)).ToList().OrderBy(x => x.QualificationCourseTitle)
        //                          select c;

        //    var pendingCourses = from Course c in Courses.Where(c => c.CourseStatus== RecordStatus.MigrationPending || c.CourseStatus== RecordStatus.BulkUloadPending)
        //                          select c;

        //    foreach (var course in filteredCourses)
        //    {
        //        var filteredCourseRuns = new List<CourseRun>();

        //        filteredCourseRuns = course.CourseRuns.ToList();
        //        filteredCourseRuns.RemoveAll(x => x.RecordStatus != RecordStatus.Live);

        //        course.CourseRuns = filteredCourseRuns;
        //    }

        //    var levelFilters = filteredCourses.GroupBy(x => x.NotionalNVQLevelv2).OrderBy(x => x.Key).ToList();

        //    var levelFiltersForDisplay = new List<QualificationLevelFilterViewModel>();

        //    var courseViewModels = new List<CourseViewModel>();

        //    if (string.IsNullOrWhiteSpace(level))
        //    {
        //        level = levelFilters.FirstOrDefault()?.Key;
        //    }
        //    else
        //    {
        //        if (!filteredCourses.Any(x => x.NotionalNVQLevelv2==level))
        //        {
        //            level = levelFilters.FirstOrDefault()?.Key;
        //        }
        //    }

        //    foreach (var levels in levelFilters)
        //    { 

        //        var lf = new QualificationLevelFilterViewModel()
        //        {
        //            Facet = levels.ToList().Count().ToString(),
        //            IsSelected = level == levels.Key,
        //            Value = levels.Key,
        //            Name = $"Level {levels.Key}",

        //        };

        //        levelFiltersForDisplay.Add(lf);

        //        foreach (var course in levels)
        //        {
        //            if (course.NotionalNVQLevelv2 == level)
        //            {
        //                var courseVM = new CourseViewModel()
        //                {
        //                    Id = course.id.ToString(),
        //                    AwardOrg = course.AwardOrgCode,
        //                    LearnAimRef = course.LearnAimRef,
        //                    NotionalNVQLevelv2 = course.NotionalNVQLevelv2,
        //                    QualificationTitle = course.QualificationCourseTitle,
        //                    QualificationType = course.QualificationType,
        //                    Facet = course.CourseRuns.Count().ToString(),
        //                    CourseRuns = course.CourseRuns.Select(y => new CourseRunViewModel
        //                        {
        //                            Id = y.id.ToString(),
        //                            CourseId = y.ProviderCourseID,
        //                            AttendancePattern = y.AttendancePattern.ToDescription(),
        //                            Cost = y.Cost.HasValue ? $"£ {y.Cost.Value:0.00}" : string.Empty,
        //                            CourseName = y.CourseName,
        //                            DeliveryMode = y.DeliveryMode.ToDescription(),
        //                            Duration = y.DurationValue.HasValue
        //                                ? $"{y.DurationValue.Value} {y.DurationUnit.ToDescription()}"
        //                                : $"0 {y.DurationUnit.ToDescription()}",
        //                            Venue = y.VenueId.HasValue
        //                                ? FormatAddress(GetVenueByIdFrom(venueResult.Value, y.VenueId.Value))
        //                                : string.Empty,
        //                            Region = y.Regions != null
        //                                ? FormattedRegionsByIds(allRegions, y.Regions)
        //                                : string.Empty,
        //                            StartDate = y.FlexibleStartDate
        //                                ? "Flexible start date"
        //                                : y.StartDate?.ToString("dd/MM/yyyy"),
        //                            StudyMode = y.StudyMode == Models.Models.Courses.StudyMode.Undefined
        //                                ? string.Empty
        //                                : y.StudyMode.ToDescription(),
        //                            Url = y.CourseURL
        //                        })
        //                        .OrderBy(y => y.CourseName)
        //                        .ToList()
        //                };

        //                courseViewModels.Add(courseVM);
        //            }
        //        }
        //    }

        //    if (string.IsNullOrWhiteSpace(level))
        //    {
        //        level = levelFiltersForDisplay.FirstOrDefault()?.Value;
        //        levelFiltersForDisplay.ForEach(x => { if (x.Value == level) { x.IsSelected = true; } });
        //    }

        //    courseViewModels.OrderBy(x => x.QualificationTitle).ToList();

        //    var notificationCourseName = string.Empty;
        //    var notificationAnchorTag = string.Empty;

        //    if (courseId.HasValue && courseId.Value != Guid.Empty)
        //    {
        //        notificationCourseName = QualificationTitleByCourseId(courseViewModels, courseId.ToString());

        //        if (courseRunId.HasValue && courseRunId.Value != Guid.Empty)
        //        {
        //            var courseRuns = courseViewModels.Find(x => x.Id == courseId.Value.ToString())?.CourseRuns;
        //            notificationCourseName = CourseNameByCourseRunId(courseRuns, courseRunId.ToString());
        //        }
        //    }

        //    if (!courseRunId.HasValue)
        //    {
        //        notificationMessage = string.Empty;
        //    }

        //    notificationAnchorTag = courseRunId.HasValue
        //        ? $"<a id=\"courseeditlink\" class=\"govuk-link\" href=\"#\" data-courseid=\"{courseId}\" data-courserunid=\"{courseRunId}\" >{notificationMessage} {notificationCourseName}</a>"
        //        : $"<a id=\"courseeditlink\" class=\"govuk-link\" href=\"#\" data-courseid=\"{courseId}\">{notificationMessage} {notificationCourseName}</a>";

        //    var viewModel = new YourCoursesViewModel
        //    {
        //        Heading = $"Level {level}",
        //        HeadingCaption = "Your courses",
        //        Courses = courseViewModels ?? new List<CourseViewModel>(),
        //        LevelFilters = levelFiltersForDisplay,
        //        NotificationTitle = notificationTitle,
        //        NotificationMessage = notificationAnchorTag,
        //        PendingCoursesCount = pendingCourses?.Count()
        //    };

        //    return View(viewModel);
        //}
    }
}