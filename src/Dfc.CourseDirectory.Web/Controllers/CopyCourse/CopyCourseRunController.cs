using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Regions;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.CopyCourse;
using Dfc.CourseDirectory.Web.ViewModels.EditCourse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Controllers.CopyCourse
{
    public class CopyCourseRunController : Controller
    {
        private readonly ILogger<CopyCourseRunController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;

        private readonly ICourseService _courseService;

        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly IVenueSearchHelper _venueSearchHelper;
        private readonly IVenueService _venueService;

        public CopyCourseRunController(
            ILogger<CopyCourseRunController> logger,
            IOptions<CourseServiceSettings> courseSearchSettings,
            IHttpContextAccessor contextAccessor,
            ICourseService courseService,
            IVenueService venueService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(courseSearchSettings, nameof(courseSearchSettings));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(venueService, nameof(venueService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _venueService = venueService;
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(string learnAimRef, string notionalNVQLevelv2, string awardOrgCode, string learnAimRefTitle, string learnAimRefTypeDesc,  Guid? courseId, Guid courseRunId, PublishMode publishMode)
        {
            int? UKPRN;

            if (_session.GetInt32("UKPRN") != null)
            {
                UKPRN = _session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            List<SelectListItem> courseRunVenues = new List<SelectListItem>();

            VenueSearchCriteria criteria = new VenueSearchCriteria(UKPRN.ToString(), null);
            var venues = await _venueService.SearchAsync(criteria);

            foreach (var venue in venues.Value.Value)
            {
                var item = new SelectListItem { Text = venue.VenueName, Value = venue.ID };
                courseRunVenues.Add(item);
            };

            var regions = _courseService.GetRegions();

            if (courseId.HasValue)
            {
                var course = await _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(courseId.Value));

                var courseRun = course.Value.CourseRuns.SingleOrDefault(cr => cr.id == courseRunId);

                if (courseRun != null)
                {
                    CopyCourseRunViewModel vm = new CopyCourseRunViewModel
                    {
                        AwardOrgCode = awardOrgCode,
                        LearnAimRef = learnAimRef,
                        LearnAimRefTitle = learnAimRefTitle,

                        CourseId = courseId.Value,
                        CourseRunId = courseRunId,
                        CourseName = courseRun?.CourseName,
                        Venues = courseRunVenues,
                        VenueId = courseRun.VenueId ?? Guid.Empty,
                        SelectRegion = regions,
                        DeliveryMode = courseRun.DeliveryMode,

                        CourseProviderReference = courseRun?.ProviderCourseID,
                        DurationUnit = courseRun.DurationUnit,
                        DurationLength = courseRun?.DurationValue?.ToString(),
                        StartDateType = courseRun.FlexibleStartDate
                            ? StartDateType.FlexibleStartDate
                            : StartDateType.SpecifiedStartDate,
                        Day = courseRun.StartDate?.Day.ToString("00"),
                        Month = courseRun.StartDate?.Month.ToString("00"),
                        Year = courseRun.StartDate?.Year.ToString("0000"),
                        StudyMode = courseRun.StudyMode,
                        Url = courseRun.CourseURL,
                        Cost = courseRun.Cost?.ToString("F"),
                        CostDescription = courseRun.CostDescription,
                        AttendanceMode = courseRun.AttendancePattern,
                        QualificationType = course.Value.QualificationType,
                        NotionalNVQLevelv2 = course.Value.NotionalNVQLevelv2,
                        PublishMode = publishMode
                    };

                    if (courseRun.Regions == null) return View("CopyCourseRun", vm);

                    foreach (var selectRegionRegionItem in vm.SelectRegion.RegionItems)
                    {
                        foreach (var subRegionItemModel in selectRegionRegionItem.SubRegion)
                        {
                            if (courseRun.Regions.Contains(subRegionItemModel.Id))
                            {
                                subRegionItemModel.Checked = true;
                            }
                        }
                    }

                    if (vm.SelectRegion.RegionItems != null && vm.SelectRegion.RegionItems.Any())
                    {
                        vm.SelectRegion.RegionItems = vm.SelectRegion.RegionItems.OrderBy(x => x.RegionName);
                        foreach (var selectRegionRegionItem in vm.SelectRegion.RegionItems)
                        {
                            selectRegionRegionItem.SubRegion = selectRegionRegionItem.SubRegion.OrderBy(x => x.SubRegionName).ToList();
                        }
                    }

                    return View("CopyCourseRun", vm);
                }
            }

            //error page
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Index(CopyCourseRunSaveViewModel model)
        {
            int? UKPRN;

            if (_session.GetInt32("UKPRN") != null)
            {
                UKPRN = _session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            if (model.CourseId.HasValue)
            {
                var courseForEdit = await _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(model.CourseId.Value));

                if (courseForEdit.IsSuccess && courseForEdit.HasValue)
                {
                    var courseRuns = courseForEdit.Value.CourseRuns.ToList();

                    var courseRunForCopy = courseRuns.SingleOrDefault(cr => cr.id == model.CourseRunId);

                    var copiedCourseRun = new CourseRun
                    {
                        id = Guid.NewGuid(),
                        DurationUnit = model.DurationUnit,
                        AttendancePattern = model.AttendanceMode,
                        DeliveryMode = model.DeliveryMode,
                        FlexibleStartDate = model.FlexibleStartDate,
                        StudyMode = model.StudyMode,
                        CostDescription = model.CostDescription,
                        CourseName = model.CourseName,
                        CourseURL = model.Url,
                        DurationValue = Convert.ToInt32(model.DurationLength),
                        ProviderCourseID = model.CourseProviderReference,
                        RecordStatus = RecordStatus.Live
                    };

                    copiedCourseRun.Cost = Convert.ToDecimal(model.Cost);
                    if (string.IsNullOrEmpty(model.Cost))
                    {
                        copiedCourseRun.Cost = null;
                    }

                    bool flexibleStartDate = true;
                    DateTime? specifiedStartDate = null;

                    if (model.StartDateType.Equals("SpecifiedStartDate",
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        string day = model.Day.Length == 1 ? string.Concat("0", model.Day) : model.Day;
                        string month = model.Month.Length == 1 ? string.Concat("0", model.Month) : model.Month;
                        string startDate = string.Format("{0}-{1}-{2}", day, month, model.Year);
                        specifiedStartDate = DateTime.ParseExact(startDate, "dd-MM-yyyy",
                            System.Globalization.CultureInfo.InvariantCulture);

                        flexibleStartDate = false;
                    }

                    copiedCourseRun.FlexibleStartDate = flexibleStartDate;
                    copiedCourseRun.StartDate = specifiedStartDate;
                    copiedCourseRun.CreatedDate=DateTime.Now;
                    copiedCourseRun.CreatedBy = User.Identity.Name;

                    switch (model.DeliveryMode)
                    {
                        case DeliveryMode.ClassroomBased:
                            copiedCourseRun.AttendancePattern = model.AttendanceMode;
                            copiedCourseRun.StudyMode = model.StudyMode;

                            copiedCourseRun.Regions = null;
                            copiedCourseRun.VenueId = model.VenueId;
                            break;
                        case DeliveryMode.WorkBased:
                            copiedCourseRun.VenueId = null;
                            copiedCourseRun.Regions = model.SelectedRegions;
                            var availableRegions = new SelectRegionModel();

                            string[] selectedRegions = availableRegions.SubRegionsDataCleanse(copiedCourseRun.Regions.ToList());

                            var subRegions = selectedRegions.Select(selectedRegion => availableRegions.GetRegionFromName(selectedRegion)).ToList();
                            copiedCourseRun.SubRegions = subRegions;

                            copiedCourseRun.AttendancePattern = AttendancePattern.Undefined;
                            copiedCourseRun.StudyMode = StudyMode.Undefined;
                            break;
                        case DeliveryMode.Online:

                            copiedCourseRun.Regions = null;
                            copiedCourseRun.VenueId = null;
                            copiedCourseRun.AttendancePattern = AttendancePattern.Undefined;
                            copiedCourseRun.StudyMode = StudyMode.Undefined;
                            break;
                    }

                    courseRuns.Add(copiedCourseRun);
                    courseForEdit.Value.CourseRuns = courseRuns;

                    var updatedCourse = await _courseService.UpdateCourseAsync(courseForEdit.Value);
                    if (updatedCourse.IsSuccess && updatedCourse.HasValue)
                    {
                        return RedirectToAction("Courses", "Provider",
                            new
                            {
                                notificationTitle = "New course added",
                                notificationMessage = "You added",
                                level = updatedCourse.Value.NotionalNVQLevelv2,
                                courseId = updatedCourse.Value.id,
                                courseRunId = copiedCourseRun.id
                            });
                    }


                }
            }

            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}