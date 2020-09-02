using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Regions;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Extensions;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseRegion;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.SelectVenue;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.CopyCourse;
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
        private const string CopyCourseRunSaveViewModelSessionKey = "CopyCourseRunSaveViewModel";
        private const string CopyCourseRunPublishedCourseSessionKey = "CopyCourseRunPublishedCourse";

        private readonly ILogger<CopyCourseRunController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly ICourseService _courseService;

        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly IVenueSearchHelper _venueSearchHelper;
        private readonly IVenueService _venueService;

        public CopyCourseRunController(
            ILogger<CopyCourseRunController> logger,
            IOptions<CourseServiceSettings> courseSearchSettings,
            IHttpContextAccessor contextAccessor,
            HtmlEncoder htmlEncoder,
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
            _htmlEncoder = htmlEncoder;
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
            if (!_session.GetInt32("UKPRN").HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var ukprn = _session.GetInt32("UKPRN").Value;

            var cachedData = _session.GetObject<CopyCourseRunViewModel>("CopyCourseRunObject");
            var course = await _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(cachedData.CourseId.Value));
            var courseRun = course.Value.CourseRuns.SingleOrDefault(cr => cr.id == cachedData.CourseRunId);
            var venues = await GetVenuesByUkprn(ukprn);

            var regions = _courseService.GetRegions();

            foreach (var subRegion in regions?.RegionItems.SelectMany(r => r.SubRegion))
            {
                subRegion.Checked = courseRun.Regions.Contains(subRegion.Id);
            }

            CopyCourseRunViewModel vm = new CopyCourseRunViewModel
            {
                AwardOrgCode = course.Value.AwardOrgCode,
                LearnAimRef = course.Value.LearnAimRef,
                LearnAimRefTitle = course.Value.QualificationCourseTitle,

                //Mode = mode,
                CourseId = course.Value.id,
                CourseRunId = courseRun.id,
                CourseName = courseRun?.CourseName,
                Venues = venues.VenueItems.Select(v => new SelectListItem { Text = v.VenueName, Value = v.Id }).ToList(),
                VenueId = courseRun.VenueId ?? null,
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
        public async Task<IActionResult> Index(Guid? courseId, Guid? courseRunId)
        {
            if (!_session.GetInt32("UKPRN").HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var ukprn = _session.GetInt32("UKPRN").Value;

            var savedModel = _session.GetObject<CopyCourseRunSaveViewModel>(CopyCourseRunSaveViewModelSessionKey);

            if (courseId.HasValue && courseRunId.HasValue)
            {
                if (savedModel != null && (savedModel.CourseId != courseId || savedModel.CourseRunId != courseRunId))
                {
                    _session.Remove(CopyCourseRunSaveViewModelSessionKey);
                    savedModel = null;
                }
            }
            else if (savedModel != null)
            {
                courseId = savedModel.CourseId;
                courseRunId = savedModel.CourseRunId;
            }
            else
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            var course = await _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(courseId.Value));

            if (!course.HasValue)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            var courseRun = course.Value.CourseRuns.SingleOrDefault(cr => cr.id == courseRunId);

            if (courseRun == null)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            var venues = await _venueService.SearchAsync(new VenueSearchCriteria(ukprn.ToString(), null));
            var regions = _courseService.GetRegions();

            foreach (var subRegion in regions?.RegionItems.SelectMany(r => r.SubRegion))
            {
                subRegion.Checked = courseRun.Regions.Contains(subRegion.Id);
            }

            var vm = new CopyCourseRunViewModel
            {
                AwardOrgCode = course.Value.AwardOrgCode,
                LearnAimRef = course.Value.LearnAimRef,
                LearnAimRefTitle = course.Value.QualificationCourseTitle,

                CourseId = courseId.Value,
                CourseRunId = courseRunId.Value,
                CourseName = courseRun?.CourseName,
                Venues = venues.Value.Value.Select(v => new SelectListItem { Text = v.VenueName, Value = v.ID }).ToList(),
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
                PublishMode = PublishMode.Summary,
                RefererAbsolutePath = Request.GetTypedHeaders().Referer?.AbsolutePath
            };

            if (savedModel != null)
            {
                vm.CourseName = savedModel.CourseName;
                vm.CourseProviderReference = savedModel.CourseProviderReference;
                vm.DeliveryMode = savedModel.DeliveryMode;
                vm.StartDateType = savedModel.StartDateType;
                vm.Day = savedModel.Day;
                vm.Month = savedModel.Month;
                vm.Year = savedModel.Year;
                vm.VenueId = savedModel.VenueId;
                vm.ChooseRegion.National = savedModel.National;
                vm.Url = savedModel.Url;
                vm.Cost = savedModel.Cost;
                vm.CostDescription = savedModel.CostDescription;
                vm.DurationLength = savedModel.DurationLength;
                vm.DurationUnit = savedModel.DurationUnit;
                vm.AttendanceMode = savedModel.AttendanceMode;
                vm.StudyMode = savedModel.StudyMode;
                vm.RefererAbsolutePath = savedModel.RefererAbsolutePath;

                if (savedModel.DeliveryMode == DeliveryMode.ClassroomBased)
                {
                    foreach (var venue in vm.Venues)
                    {
                        venue.Selected = venue.Value == savedModel.VenueId.ToString();
                    }
                }

                if (savedModel.DeliveryMode == DeliveryMode.WorkBased && !savedModel.National)
                {
                    foreach (var subRegion in regions?.RegionItems.SelectMany(r => r.SubRegion))
                    {
                        subRegion.Checked = savedModel.SelectedRegions.Contains(subRegion.Id);
                    }
                }
            }

            return View("CopyCourseRun", vm);
        }

        [HttpPost]
        public IActionResult Index(CopyCourseRunSaveViewModel model)
        {
            if (!_session.GetInt32("UKPRN").HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            if (model == null)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            _session.SetObject(CopyCourseRunSaveViewModelSessionKey, model);

            return RedirectToAction(nameof(Summary));
        }

        [HttpGet]
        public async Task<IActionResult> Summary()
        {
            if (!_session.GetInt32("UKPRN").HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var ukprn = (int)_session.GetInt32("UKPRN");

            var model = _session.GetObject<CopyCourseRunSaveViewModel>(CopyCourseRunSaveViewModelSessionKey);

            if (model == null)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            var availableRegions = new Lazy<SelectRegionModel>(() => _courseService.GetRegions());
            var regionIds = new Lazy<string[]>(() => availableRegions.Value.SubRegionsDataCleanse(model.SelectedRegions.ToList()));

            var summaryViewModel = new CopyCourseRunSummaryViewModel
            {
                LearnAimRefTitle = model.LearnAimRefTitle,
                CourseName = model.CourseName,
                CourseProviderReference = model.CourseProviderReference,
                DeliveryMode = model.DeliveryMode,
                StartDate = model.StartDateType == StartDateType.FlexibleStartDate
                    ? "Flexible"
                    : $"{model.Day}/{model.Month}/{model.Year}",
                Venues = model.DeliveryMode == DeliveryMode.ClassroomBased
                    ? (await GetVenuesByUkprn(ukprn)).VenueItems
                        .Where(v => v.Id == model.VenueId.ToString())
                        .Select(v => v.VenueName)
                    : Enumerable.Empty<string>(),
                Regions = model.DeliveryMode == DeliveryMode.WorkBased
                    ? model.National
                        ? new[] { "National"}
                        : availableRegions.Value.RegionItems
                            .Select(r => new { r.Id, r.RegionName })
                            .Concat(availableRegions.Value.RegionItems
                                .SelectMany(r => r.SubRegion)
                                .Select(r => new { r.Id, RegionName = r.SubRegionName }))
                            .Where(r => regionIds.Value.Contains(r.Id))
                            .Select(r => r.RegionName)
                    : Enumerable.Empty<string>(),
                Url = model.Url,
                Cost = model.Cost,
                CostDescription = model.CostDescription,
                CourseLength = $"{model.DurationLength} {model.DurationUnit}",
                AttendancePattern = model.AttendanceMode,
                StudyMode = model.StudyMode
            };

            return View(summaryViewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Publish()
        {
            if (!_session.GetInt32("UKPRN").HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var model = _session.GetObject<CopyCourseRunSaveViewModel>(CopyCourseRunSaveViewModelSessionKey);

            if (model == null)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            if (!model.CourseId.HasValue)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            var course = await _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(model.CourseId.Value));

            if (!course.IsSuccess || !course.HasValue)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            var courseRun = new CourseRun
            {
                id = Guid.NewGuid(),
                DurationUnit = model.DurationUnit,
                AttendancePattern = model.AttendanceMode,
                DeliveryMode = model.DeliveryMode,
                StudyMode = model.StudyMode,
                CostDescription = _htmlEncoder.Encode(model.CostDescription ?? ""),
                CourseName = _htmlEncoder.Encode(model.CourseName ?? ""),
                CourseURL = model.Url,
                DurationValue = Convert.ToInt32(model.DurationLength),
                ProviderCourseID = _htmlEncoder.Encode(model.CourseProviderReference ?? ""),
                RecordStatus = RecordStatus.Live,
                Cost = !string.IsNullOrEmpty(model.Cost) ? Convert.ToDecimal(model.Cost) : (decimal?)null,
                FlexibleStartDate = model.StartDateType != StartDateType.SpecifiedStartDate,
                CreatedDate = DateTime.Now,
                CreatedBy = User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault(),
            };

            if (!courseRun.FlexibleStartDate)
            {
                courseRun.StartDate = DateTime.ParseExact(
                    $"{int.Parse(model.Day):00}-{int.Parse(model.Month):00}-{model.Year}",
                    "dd-MM-yyyy",
                    System.Globalization.CultureInfo.InvariantCulture);
            }

            switch (model.DeliveryMode)
            {
                case DeliveryMode.ClassroomBased:
                    courseRun.AttendancePattern = model.AttendanceMode;
                    courseRun.StudyMode = model.StudyMode;

                    courseRun.Regions = null;
                    courseRun.VenueId = model.VenueId;
                    break;
                case DeliveryMode.WorkBased:
                    courseRun.VenueId = null;

                    var availableRegions = new SelectRegionModel();

                    if (model.National)
                    {
                        courseRun.National = true;
                        courseRun.Regions = availableRegions.RegionItems.Select(x => x.Id).ToList();
                    }
                    else
                    {
                        courseRun.National = false;
                        courseRun.Regions = model.SelectedRegions;
                        string[] selectedRegions = availableRegions.SubRegionsDataCleanse(courseRun.Regions.ToList());

                        var subRegions = selectedRegions.Select(selectedRegion => availableRegions.GetSubRegionItemByRegionCode(selectedRegion)).ToList();
                        courseRun.SubRegions = subRegions;

                        courseRun.AttendancePattern = AttendancePattern.Undefined;
                        courseRun.StudyMode = StudyMode.Undefined;
                    }
                    break;
                case DeliveryMode.Online:

                    courseRun.Regions = null;
                    courseRun.VenueId = null;
                    courseRun.AttendancePattern = AttendancePattern.Undefined;
                    courseRun.StudyMode = StudyMode.Undefined;
                    break;
            }

            course.Value.CourseRuns = course.Value.CourseRuns.Append(courseRun);

            try
            {
                var result = await _courseService.UpdateCourseAsync(course.Value);

                if (!result.IsSuccess || !result.HasValue)
                {
                    return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                }

                _session.SetObject(CopyCourseRunPublishedCourseSessionKey, new PublishedCourseViewModel
                {
                    CourseId = course.Value.id,
                    CourseRunId = courseRun.id,
                    CourseName = courseRun.CourseName
                });

                return RedirectToAction("Published");
            }
            finally
            {
                _session.Remove("NewAddedVenue");
                _session.Remove("Option");
                _session.Remove(CopyCourseRunSaveViewModelSessionKey);
            }
        }

        [HttpGet]
        public IActionResult Published()
        {
            var publishedCourse = _session.GetObject<PublishedCourseViewModel>(CopyCourseRunPublishedCourseSessionKey);

            if (publishedCourse == null)
            {
                return RedirectToAction("Index", "Home");
            }

            _session.Remove(CopyCourseRunPublishedCourseSessionKey);

            return View(publishedCourse);
        }
    }
}