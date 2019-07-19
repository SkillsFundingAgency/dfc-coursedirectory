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
using Dfc.CourseDirectory.Web.Extensions;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.SelectVenue;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.CopyCourse;
using Dfc.CourseDirectory.Web.ViewModels.EditCourse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseRegion;

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
            _session.SetString("Option", "AddNewVenueForCopy");

            CopyCourseRunViewModel vm = new CopyCourseRunViewModel
            {
                AwardOrgCode = model.AwardOrgCode,
                LearnAimRef = model.LearnAimRef,
                LearnAimRefTitle = model.LearnAimRefTitle,
                NotionalNVQLevelv2 = model.NotionalNVQLevelv2,
                PublishMode = model.Mode,
                CourseId = model.CourseId,
                CourseRunId = model.CourseRunId,
                CourseName = model.CourseName,
                VenueId = model.VenueId,
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

            _session.SetObject("CopyCourseRunObject", vm);

            return Json(Url.Action("AddVenue", "Venues"));
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


            var cachedData = _session.GetObject<CopyCourseRunViewModel>("CopyCourseRunObject");


            var course = await _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(cachedData.CourseId.Value));

            var courseRun = course.Value.CourseRuns.SingleOrDefault(cr => cr.id == cachedData.CourseRunId);


            CopyCourseRunViewModel vm = new CopyCourseRunViewModel
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
                    Regions = regions,
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
                NotionalNVQLevelv2 = course.Value.NotionalNVQLevelv2
            };

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

            if (courseRun.Regions == null) return View("CopyCourseRun", vm);

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

         

            return View("CopyCourseRun", vm);




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
                        PublishMode = publishMode
                    };

                    if (courseRun.Regions == null) return View("CopyCourseRun", vm);

                    foreach (var selectRegionRegionItem in vm.ChooseRegion.Regions.RegionItems)
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
                    copiedCourseRun.CreatedBy = User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault();

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
                            
                            var availableRegions = new SelectRegionModel();

                            if(model.National)
                            {
                                copiedCourseRun.National = true;
                                copiedCourseRun.Regions = availableRegions.RegionItems.Select(x => x.Id).ToList();
                            }
                            else
                            {
                                copiedCourseRun.National = false;
                                copiedCourseRun.Regions = model.SelectedRegions;
                                string[] selectedRegions = availableRegions.SubRegionsDataCleanse(copiedCourseRun.Regions.ToList());

                                var subRegions = selectedRegions.Select(selectedRegion => availableRegions.GetSubRegionItemByRegionCode(selectedRegion)).ToList();
                                copiedCourseRun.SubRegions = subRegions;

                                copiedCourseRun.AttendancePattern = AttendancePattern.Undefined;
                                copiedCourseRun.StudyMode = StudyMode.Undefined;
                            }
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

                    _session.Remove("NewAddedVenue");
                    _session.Remove("Option");

                    var updatedCourse = await _courseService.UpdateCourseAsync(courseForEdit.Value);
                    if (updatedCourse.IsSuccess && updatedCourse.HasValue)
                    {
                        return RedirectToAction("index", "ProviderCourses",
                            new
                            {
                                notificationTitle = "New course added",
                                notificationMessage = "You added",
                                courseRunId = copiedCourseRun.id
                            });
                    }


                }
            }

            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}