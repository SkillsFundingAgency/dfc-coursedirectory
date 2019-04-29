using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseTextService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.EditCourse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Dfc.CourseDirectory.Models.Models.Regions;

namespace Dfc.CourseDirectory.Web.Controllers.EditCourse
{
    public class EditCourseRunController : Controller
    {
        private readonly ILogger<EditCourseRunController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;

        private readonly ICourseService _courseService;

        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly IVenueSearchHelper _venueSearchHelper;
        private readonly IVenueService _venueService;

        public EditCourseRunController(
            ILogger<EditCourseRunController> logger,
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
        public async Task<IActionResult> Index(string learnAimRef, string notionalNVQLevelv2, string awardOrgCode, string learnAimRefTitle, string learnAimRefTypeDesc, Guid? courseId, Guid courseRunId, PublishMode mode)
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
                    EditCourseRunViewModel vm = new EditCourseRunViewModel
                    {
                        AwardOrgCode = awardOrgCode,
                        LearnAimRef = learnAimRef,
                        LearnAimRefTitle = learnAimRefTitle,

                        Mode = mode,
                        CourseId = courseId.Value,
                        CourseRunId = courseRunId,
                        CourseName = courseRun?.CourseName,
                        Venues = courseRunVenues,
                        VenueId = courseRun.VenueId ?? (Guid?)null,
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
                        CurrentCourseRunDate = courseRun.StartDate
                    };

                    if (mode == PublishMode.BulkUpload || mode == PublishMode.DataQualityIndicator)
                    {
                        vm.ValPastDateRef = DateTime.Now;
                        vm.ValPastDateMessage = "Start Date cannot be earlier than today’s date";

                        //var venueExists = vm.Venues.Any(x => x.Text == )
                    }
                    else
                    {
                        vm.ValPastDateRef = courseRun.StartDate ?? DateTime.Now;
                        if (courseRun.FlexibleStartDate == true)
                        {
                            vm.ValPastDateMessage = "Start Date cannot be earlier than today’s date";
                        }
                        else
                        {
                            vm.ValPastDateMessage = "New Start Date cannot be before the pre-edited Start Date";
                        }

                    }

                    if (courseRun.Regions == null) return View("EditCourseRun", vm);
                    
                    foreach (var selectRegionRegionItem in vm.SelectRegion.RegionItems.OrderBy(x=>x.RegionName))
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

                    return View("EditCourseRun", vm);
                }
            }

            //error page
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Index(EditCourseRunSaveViewModel model)
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
                    var regions = new List<string>();
                    var courseRunForEdit = courseForEdit.Value.CourseRuns.SingleOrDefault(cr => cr.id == model.CourseRunId);

                    courseRunForEdit.DurationUnit = model.DurationUnit;
                    courseRunForEdit.DeliveryMode = model.DeliveryMode;
                    courseRunForEdit.FlexibleStartDate = model.FlexibleStartDate;

                    courseRunForEdit.Cost = Convert.ToDecimal(model.Cost);
                    if (string.IsNullOrEmpty(model.Cost))
                    {
                        courseRunForEdit.Cost = null;
                    }

                    courseRunForEdit.CostDescription = model.CostDescription;
                    courseRunForEdit.CourseName = model.CourseName;
                    courseRunForEdit.CourseURL = model.Url;
                    courseRunForEdit.DurationValue = Convert.ToInt32(model.DurationLength);
                    courseRunForEdit.ProviderCourseID = model.CourseProviderReference;

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

                    courseRunForEdit.FlexibleStartDate = flexibleStartDate;
                    courseRunForEdit.StartDate = specifiedStartDate;
                    courseRunForEdit.UpdatedDate = DateTime.Now;
                    courseRunForEdit.UpdatedBy = User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault();

                    switch (model.DeliveryMode)
                    {
                        case DeliveryMode.ClassroomBased:

                            courseRunForEdit.Regions = null;
                            courseRunForEdit.VenueId = model.VenueId;

                            courseRunForEdit.AttendancePattern = model.AttendanceMode;
                            courseRunForEdit.StudyMode = model.StudyMode;

                            break;
                        case DeliveryMode.WorkBased:
                            courseRunForEdit.VenueId = null;
                            courseRunForEdit.Regions = model.SelectedRegions;
                            var availableRegions = new SelectRegionModel();

                            string[] selectedRegions = availableRegions.SubRegionsDataCleanse(courseRunForEdit.Regions.ToList());

                            var subRegions = selectedRegions.Select(selectedRegion => availableRegions.GetRegionFromName(selectedRegion)).ToList();
                            courseRunForEdit.SubRegions = subRegions;
                            courseRunForEdit.AttendancePattern = AttendancePattern.Undefined;
                            courseRunForEdit.StudyMode = StudyMode.Undefined;

                            break;
                        case DeliveryMode.Online:

                            courseRunForEdit.Regions = null;
                            courseRunForEdit.VenueId = null;

                            courseRunForEdit.AttendancePattern = AttendancePattern.Undefined;
                            courseRunForEdit.StudyMode = StudyMode.Undefined;

                            break;
                    }

                    //todo when real data
                    switch (model.Mode)
                    {
                        case PublishMode.BulkUpload:
                            courseRunForEdit.RecordStatus = RecordStatus.BulkUploadReadyToGoLive;
                            break;
                        case PublishMode.Migration:
                            courseRunForEdit.RecordStatus = RecordStatus.MigrationReadyToGoLive;
                            break;
                        case PublishMode.DataQualityIndicator:
                        default:
                            courseRunForEdit.RecordStatus = RecordStatus.Live;
                            break;
                    }

                    var updatedCourse = await _courseService.UpdateCourseAsync(courseForEdit.Value);
                    if (updatedCourse.IsSuccess && updatedCourse.HasValue)
                    {

                        switch (model.Mode)
                        {
                            case PublishMode.BulkUpload:
                            case PublishMode.Migration:
                                return RedirectToAction("Index", "PublishCourses",
                                new
                                {
                                    publishMode = model.Mode,
                                    courseId = model.CourseId,
                                    courseRunId = model.CourseRunId

                                });
                            case PublishMode.DataQualityIndicator:
                                return RedirectToAction("Index", "PublishCourses",
                                 new
                                 {
                                     publishMode = model.Mode,
                                     courseId = model.CourseId,
                                     courseRunId = model.CourseRunId,
                                     NotificationTitle = model.CourseName + " has been updated",
                                     NotificationMessage = "Start date edited"
                                 });
                            default:
                                return RedirectToAction("index", "ProviderCourses",
                                    new
                                    {
                                        NotificationTitle = "Course edited",
                                        NotificationMessage = "You edited",
                                        courseRunId = model.CourseRunId
                                    });

                        }
                    }


                }
            }

            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}