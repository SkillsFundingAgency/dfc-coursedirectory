using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Services.Models.Regions;
using Dfc.CourseDirectory.Web.Extensions;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseRegion;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.SelectVenue;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.EditCourse;
using Dfc.CourseDirectory.WebV2;
using Flurl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Controllers.EditCourse
{
    public class EditCourseRunController : Controller
    {
        private readonly HtmlEncoder _htmlEncoder;
        private readonly ICourseService _courseService;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        private ISession Session => HttpContext.Session;
        private readonly IProviderContextProvider _providerContextProvider;

        private const string SessionVenues = "Venues";
        private const string SessionRegions = "Regions";

        public EditCourseRunController(
            IOptions<CourseServiceSettings> courseSearchSettings,
            HtmlEncoder htmlEncoder,
            ICourseService courseService,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderContextProvider providerContextProvider)
        {
            if (courseSearchSettings == null)
            {
                throw new ArgumentNullException(nameof(courseSearchSettings));
            }

            if (courseService == null)
            {
                throw new ArgumentNullException(nameof(courseService));
            }

            _courseService = courseService;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _htmlEncoder = htmlEncoder;
            _providerContextProvider = providerContextProvider;
        }

        [Authorize]
        public IActionResult AddNewVenue(CourseRunRequestModel model)
        {

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

          
            Session.SetObject("EditCourseRunObject", vm);


            return Json(new Url(Url.Action("Index", "AddVenue", new { returnUrl = Url.Action("Reload", "EditCourseRun") }))
                .WithProviderContext(_providerContextProvider.GetProviderContext(withLegacyFallback: true))
                .ToString());


        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Reload()
        {
            int? UKPRN;

            if (Session.GetInt32("UKPRN") != null)
            {
                UKPRN = Session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            List<SelectListItem> courseRunVenues = new List<SelectListItem>();

            var venues = await GetVenuesForProvider();

            foreach (var venue in venues.VenueItems)
            {
                var item = new SelectListItem { Text = venue.VenueName, Value = venue.Id };
                courseRunVenues.Add(item);
            }

            var regions = _courseService.GetRegions();


            var cachedData = Session.GetObject<EditCourseRunViewModel>("EditCourseRunObject");


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

            vm.ValPastDateRef = DateTime.Now;
            vm.ValPastDateMessage = "Start Date cannot be earlier than today�s date";

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
            int? UKPRN;

            if (Session.GetInt32("UKPRN") != null)
            {
                UKPRN = Session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            List<SelectListItem> courseRunVenues = new List<SelectListItem>();

            //VenueSearchCriteria criteria = new VenueSearchCriteria(UKPRN.ToString(), null);
            var venues = await GetVenuesForProvider();

            foreach (var venue in venues.VenueItems)
            {
                var item = new SelectListItem { Text = venue.VenueName, Value = venue.Id };
                courseRunVenues.Add(item);
            };

            var regions = _courseService.GetRegions();

            Session.SetObject(SessionVenues, venues);
            Session.SetObject(SessionRegions, regions);

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
                            National = courseRun.DeliveryMode != DeliveryMode.WorkBased ? null : courseRun.National,
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

                    vm.ValPastDateRef = DateTime.Now;
                    vm.ValPastDateMessage = "Start Date cannot be earlier than today�s date";

                    if (courseRun.Regions == null) return View("EditCourseRun", vm);

                    foreach (var selectRegionRegionItem in vm.ChooseRegion.Regions.RegionItems.OrderBy(x => x.RegionName))
                    {
                        //If Region is returned, check for existence of any subregions
                        if (courseRun.Regions.Contains(selectRegionRegionItem.Id))
                        {
                            var subregionsInList = from subRegion in selectRegionRegionItem.SubRegion
                                         where courseRun.Regions.Contains(subRegion.Id)
                                         select subRegion;

                            //If true, then ignore subregions
                            if (subregionsInList.Count() > 0)
                            {
                                foreach(var subRegion in subregionsInList)
                                {
                                    courseRun.Regions = courseRun.Regions.Where(x => (x != subRegion.Id)).ToList();
                                    
                                }
                            }
                            //If false, then tick all subregions
                            foreach (var subRegionItemModel in selectRegionRegionItem.SubRegion)
                            {
                                subRegionItemModel.Checked = true;
                            }
                            
                        }
                        //Else, just tick the one subregion per item
                        else
                        {
                            foreach (var subRegionItemModel in selectRegionRegionItem.SubRegion)
                            {
                                if (courseRun.Regions.Contains(subRegionItemModel.Id))
                                {
                                    subRegionItemModel.Checked = true;
                                }
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

            if (Session.GetInt32("UKPRN") != null)
            {
                UKPRN = Session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            if (model.CourseId.HasValue)
            {
                var courseForEdit = await _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(model.CourseId.Value));

                if (courseForEdit.IsSuccess)
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

                    courseRunForEdit.CostDescription = _htmlEncoder.Encode(model.CostDescription ?? "");
                    courseRunForEdit.CourseName = _htmlEncoder.Encode(model.CourseName);
                    courseRunForEdit.CourseURL = model.Url;
                    courseRunForEdit.DurationValue = Convert.ToInt32(model.DurationLength);
                    courseRunForEdit.ProviderCourseID = _htmlEncoder.Encode(model.CourseProviderReference??"");

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
                    //Set to null by default
                    courseRunForEdit.National = null;
                    switch (model.DeliveryMode)
                    {
                        case DeliveryMode.ClassroomBased:
                            courseRunForEdit.National = null;
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
                                courseRunForEdit.National = false;
                                courseRunForEdit.Regions = model.SelectedRegions;
                                string[] selectedRegions = availableRegions.SubRegionsDataCleanse(courseRunForEdit.Regions.ToList());
                                var subRegions = selectedRegions.Select(selectedRegion => availableRegions.GetSubRegionItemByRegionCode(selectedRegion)).ToList();
                                courseRunForEdit.SubRegions = subRegions;
                                courseRunForEdit.AttendancePattern = AttendancePattern.Undefined;
                                courseRunForEdit.StudyMode = StudyMode.Undefined;
                            }
                            break;
                        case DeliveryMode.Online:

                            courseRunForEdit.Regions = null;
                            courseRunForEdit.VenueId = null;
                            courseRunForEdit.National = null;
                            courseRunForEdit.AttendancePattern = AttendancePattern.Undefined;
                            courseRunForEdit.StudyMode = StudyMode.Undefined;

                            break;
                    }

                    // Check if courserun has any errors
                    courseRunForEdit.ValidationErrors = _courseService.ValidateCourseRun(courseRunForEdit, ValidationMode.MigrateCourse).Select(x => x.Value);
                    courseForEdit.Value.ValidationErrors = _courseService.ValidateCourse(courseForEdit.Value).Select(x => x.Value);

                    // Check if course run has issues
                    bool isCourseValid = !(courseForEdit.Value.ValidationErrors != null && courseForEdit.Value.ValidationErrors.Any());
                    bool isValidCourseRun = !(courseRunForEdit.ValidationErrors != null && courseRunForEdit.ValidationErrors.Any());

                    //todo when real data
                    switch (model.Mode)
                    {
                        case PublishMode.BulkUpload:
                            courseRunForEdit.RecordStatus = RecordStatus.BulkUploadReadyToGoLive;
                            break;
                        case PublishMode.DataQualityIndicator:
                        default:
                            courseRunForEdit.RecordStatus = RecordStatus.Live;
                            break;
                    }

                    Session.Remove("NewAddedVenue");
                    Session.Remove("Option");

                    var status = courseForEdit.Value.CourseStatus;

                    var message = string.Empty;

                    RecordStatus[] validStatuses = new[] { RecordStatus.Live };

                    // Coures run is valid course is invalid, course run is fixed
                    if(courseRunForEdit.RecordStatus == RecordStatus.MigrationReadyToGoLive && !isCourseValid)
                    {
                        message = $"'{courseRunForEdit.CourseName}' was successfully fixed";
                    }
                    
                    // Course is valid and ALL course runs are fixed (course runs = Live), course can be publisehd
                    if(isCourseValid && !(courseForEdit.Value.CourseRuns.Where(x => !validStatuses.Contains(x.RecordStatus)).Any()))
                    {
                        message = $"'{courseForEdit.Value.QualificationCourseTitle}' was successfully fixed and published.";
                    }


                    var updatedCourse = await _courseService.UpdateCourseAsync(courseForEdit.Value);

                    if (updatedCourse.IsSuccess)
                    {
                        switch (model.Mode)
                        {
                            case PublishMode.BulkUpload:
                                return RedirectToAction("Index", "PublishCourses",
                                new
                                {
                                    publishMode = model.Mode,
                                    courseId = model.CourseId,
                                    courseRunId = model.CourseRunId,
                                    notificationTitle = ""
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
                                TempData[TempDataKeys.ShowCourseUpdatedNotification] = true;
                                return RedirectToAction("Index", "CourseSummary",
                                    new
                                    {
                                        courseId = model.CourseId,
                                        courseRunId = model.CourseRunId
                                    });
                        }
                    }
                }
            }

            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<SelectVenueModel> GetVenuesForProvider()
        {
            var providerContext = _providerContextProvider.GetProviderContext(withLegacyFallback: true);

            var selectVenue = new SelectVenueModel
            {
                LabelText = "Select course venue",
                HintText = "Select all that apply.",
                AriaDescribedBy = "Select all that apply.",
                Ukprn = providerContext.ProviderInfo.Ukprn
            };

            var venues = await _sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = providerContext.ProviderInfo.ProviderId });

            selectVenue.VenueItems = venues.Select(v => new VenueItemModel()
            {
                Id = v.VenueId.ToString(),
                VenueName = v.VenueName
            }).ToList();

            if (selectVenue.VenueItems.Count == 1)
            {
                selectVenue.HintText = string.Empty;
                selectVenue.AriaDescribedBy = string.Empty;
            }

            return selectVenue;
        }
    }
}
