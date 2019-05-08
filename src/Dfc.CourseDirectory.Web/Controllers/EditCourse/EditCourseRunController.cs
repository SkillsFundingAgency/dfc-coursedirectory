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
using Dfc.CourseDirectory.Web.Extensions;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.SelectVenue;
using Microsoft.ApplicationInsights;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseRegion;

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

        private const string SessionVenues = "Venues";
        private const string SessionRegions = "Regions";
        private const string SessionAddCourseSection1 = "AddCourseSection1";
        private const string SessionAddCourseSection2 = "AddCourseSection2";
        private const string SessionLastAddCoursePage = "LastAddCoursePage";
        private const string SessionSummaryPageLoadedAtLeastOnce = "SummaryLoadedAtLeastOnce";

        public EditCourseRunController(
            ILogger<EditCourseRunController> logger,
            IOptions<CourseServiceSettings> courseSearchSettings,
            IHttpContextAccessor contextAccessor,
            ICourseService courseService,
            IVenueService venueService,
            IVenueSearchHelper venueSearchHelper)
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
            _venueSearchHelper = venueSearchHelper;
        }

        [Authorize]
        public IActionResult AddNewVenue(CourseRunRequestModel model)
        {

            _session.SetString("Option", "AddNewVenueForEdit");
            EditCourseRunViewModel vm = new EditCourseRunViewModel
            {
                AwardOrgCode = model.AwardOrgCode,
                LearnAimRef = model.LearnAimRef,
                LearnAimRefTitle = model.LearnAimRefTitle,
                NotionalNVQLevelv2 = model.NotionalNVQLevelv2,
                Mode = model.Mode,
                CourseId = model.CourseId,
                CourseRunId = model.CourseRunId,
                CourseName = model.CourseName,
                //Venues = availableVenues
                VenueId = model.VenueId,
                //SelectRegion = regions,
                DeliveryMode = model.DeliveryMode,

                CourseProviderReference = model.CourseProviderReference,
                DurationUnit = model.DurationUnit,
                DurationLength = model.DurationLength.ToString(),
                StartDateType = model.StartDateType.ToUpper() == "SPECIFIEDSTARTDATE"
                    ? StartDateType.SpecifiedStartDate
                    : StartDateType.FlexibleStartDate,
                Day = model.Day,
                Month = model.Month,
                Year = model.Year,
                StudyMode = model.StudyMode,
                Url = model.Url,
                Cost = model.Cost.ToString(),
                CostDescription = model.CostDescription,
                AttendanceMode = model.AttendanceMode,
            };

          
            _session.SetObject("EditCourseRunObject", vm);


            return Json(Url.Action("AddVenue", "Venues"));


        }

        private async Task<SelectVenueModel> GetVenuesByUkprn(int ukprn)
        {
            var selectVenue = new SelectVenueModel
            {
                LabelText = "Select course venue",
                HintText = "Select all that apply.",
                AriaDescribedBy = "Select all that apply.",
                Ukprn = ukprn
            };
            var requestModel = new VenueSearchRequestModel { SearchTerm = ukprn.ToString() };
            var criteria = _venueSearchHelper.GetVenueSearchCriteria(requestModel);
            var result = await _venueService.SearchAsync(criteria);
            if (result.IsSuccess && result.HasValue)
            {
                var items = _venueSearchHelper.GetVenueSearchResultItemModels(result.Value.Value);
                var venueItems = new List<VenueItemModel>();

                foreach (var venueSearchResultItemModel in items)
                {
                    venueItems.Add(new VenueItemModel
                    {
                        Id = venueSearchResultItemModel.Id,
                        VenueName = venueSearchResultItemModel.VenueName
                    });
                }

                selectVenue.VenueItems = venueItems;
                if (venueItems.Count == 1)
                {
                    selectVenue.HintText = string.Empty;
                    selectVenue.AriaDescribedBy = string.Empty;
                }
            }

            return selectVenue;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Reload()
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

            var venues = await GetVenuesByUkprn(UKPRN.Value);

            foreach (var venue in venues.VenueItems)
            {
                var item = new SelectListItem { Text = venue.VenueName, Value = venue.Id };
                courseRunVenues.Add(item);
            }

            var regions = _courseService.GetRegions();


            var cachedData = _session.GetObject<EditCourseRunViewModel>("EditCourseRunObject");


            var course = await _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(cachedData.CourseId.Value));

            var courseRun = course.Value.CourseRuns.SingleOrDefault(cr => cr.id == cachedData.CourseRunId);


            EditCourseRunViewModel vm = new EditCourseRunViewModel
            {
                AwardOrgCode = cachedData.AwardOrgCode,
                LearnAimRef = cachedData.LearnAimRef,
                LearnAimRefTitle = cachedData.LearnAimRefTitle,

                //Mode = mode,
                CourseId = course.Value.id,
                CourseRunId = courseRun.id,
                CourseName = courseRun?.CourseName,
                Venues = courseRunVenues,
                VenueId = courseRun.VenueId ?? (Guid?)null,
                ChooseRegion = new ChooseRegionModel
                {
                    National = courseRun.National,
                    Regions = regions
                },
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

            if (cachedData.Mode == PublishMode.BulkUpload || cachedData.Mode == PublishMode.DataQualityIndicator)
            {
                vm.ValPastDateRef = DateTime.Now;
                vm.ValPastDateMessage = "Start Date cannot be earlier than today’s date";

                //var venueExists = vm.Venues.Any(x => x.Text == )
            }
            else
            {
                vm.ValPastDateRef = courseRun.StartDate ?? DateTime.Now;
                vm.ValPastDateMessage = courseRun.FlexibleStartDate == true ? "Start Date cannot be earlier than today’s date" : "New Start Date cannot be before the pre-edited Start Date";
            }

            vm.CourseName = cachedData.CourseName;
            vm.AttendanceMode = cachedData.AttendanceMode;
            vm.Cost = cachedData.Cost;
            vm.CostDescription = cachedData.CostDescription;
            vm.CourseProviderReference = cachedData.CourseProviderReference;
            vm.Day = cachedData.Day;
            vm.Month = cachedData.Month;
            vm.Year = cachedData.Year;
            vm.DeliveryMode = cachedData.DeliveryMode;
            vm.DurationLength = cachedData.DurationLength;
            vm.DurationUnit = cachedData.DurationUnit;
            vm.StartDateType = cachedData.StartDateType;
            vm.StudyMode = cachedData.StudyMode;
            vm.Url = cachedData.Url;
            vm.VenueId = cachedData.VenueId;

            if (courseRun.Regions == null) return View("EditCourseRun", vm);

            foreach (var selectRegionRegionItem in vm.ChooseRegion.Regions.RegionItems.OrderBy(x => x.RegionName))
            {
                foreach (var subRegionItemModel in selectRegionRegionItem.SubRegion)
                {
                    if (courseRun.Regions.Contains(subRegionItemModel.Id))
                    {
                        subRegionItemModel.Checked = true;
                    }
                }
            }

            if (vm.ChooseRegion.Regions.RegionItems != null && vm.ChooseRegion.Regions.RegionItems.Any())
            {
                vm.ChooseRegion.Regions.RegionItems = vm.ChooseRegion.Regions.RegionItems.OrderBy(x => x.RegionName);
                foreach (var selectRegionRegionItem in vm.ChooseRegion.Regions.RegionItems)
                {
                    selectRegionRegionItem.SubRegion =
                        selectRegionRegionItem.SubRegion.OrderBy(x => x.SubRegionName).ToList();
                }
            }

          

            return View("EditCourseRun", vm);




        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(string learnAimRef, string notionalNVQLevelv2, string awardOrgCode, string learnAimRefTitle, string learnAimRefTypeDesc, Guid? courseId, Guid courseRunId, PublishMode mode)
        {

            _session.SetString("Option", "AddNewVenueForEdit");

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

            //VenueSearchCriteria criteria = new VenueSearchCriteria(UKPRN.ToString(), null);
            var venues = await GetVenuesByUkprn(UKPRN.Value);

            foreach (var venue in venues.VenueItems)
            {
                var item = new SelectListItem { Text = venue.VenueName, Value = venue.Id };
                courseRunVenues.Add(item);
            };

            var regions = _courseService.GetRegions();

            _session.SetObject(SessionVenues, venues);
            _session.SetObject(SessionRegions, regions);

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
                        ChooseRegion = new ChooseRegionModel
                        {
                            National = courseRun.National,
                            Regions = regions
                        },
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

                    foreach (var selectRegionRegionItem in vm.ChooseRegion.Regions.RegionItems.OrderBy(x => x.RegionName))
                    {
                        foreach (var subRegionItemModel in selectRegionRegionItem.SubRegion)
                        {
                            if (courseRun.Regions.Contains(subRegionItemModel.Id))
                            {
                                subRegionItemModel.Checked = true;
                            }
                        }
                    }

                    if (vm.ChooseRegion.Regions.RegionItems != null && vm.ChooseRegion.Regions.RegionItems.Any())
                    {
                        vm.ChooseRegion.Regions.RegionItems = vm.ChooseRegion.Regions.RegionItems.OrderBy(x => x.RegionName);
                        foreach (var selectRegionRegionItem in vm.ChooseRegion.Regions.RegionItems)
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
                            var availableRegions = new SelectRegionModel();
                            if (model.National)
                            {
                                courseRunForEdit.National = true;
                                courseRunForEdit.Regions = availableRegions.RegionItems.Select(x => x.Id).ToList();
                            }
                            else
                            {
                                courseRunForEdit.Regions = model.SelectedRegions;                           
                                string[] selectedRegions = availableRegions.SubRegionsDataCleanse(courseRunForEdit.Regions.ToList());
                                var subRegions = selectedRegions.Select(selectedRegion => availableRegions.GetRegionFromName(selectedRegion)).ToList();
                                courseRunForEdit.SubRegions = subRegions;
                                courseRunForEdit.AttendancePattern = AttendancePattern.Undefined;
                                courseRunForEdit.StudyMode = StudyMode.Undefined;
                            }
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

                    _session.Remove("NewAddedVenue");
                    _session.Remove("Option");

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