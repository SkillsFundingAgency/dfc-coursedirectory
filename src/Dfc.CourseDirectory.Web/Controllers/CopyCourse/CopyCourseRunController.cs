using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation.CourseValidation;
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
using Dfc.CourseDirectory.Web.ViewModels.CopyCourse;
using Dfc.CourseDirectory.WebV2;
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation;
using Flurl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using OneOf.Types;

namespace Dfc.CourseDirectory.Web.Controllers.CopyCourse
{
    public class CopyCourseRunController : Controller
    {
        private const string CopyCourseRunSaveViewModelSessionKey = "CopyCourseRunSaveViewModel";
        private const string CopyCourseRunPublishedCourseSessionKey = "CopyCourseRunPublishedCourse";

        private readonly ILogger<CopyCourseRunController> _logger;
        private readonly ICourseService _courseService;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        private ISession _session => HttpContext.Session;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;
        private readonly IRegionCache _regionCache;

        public CopyCourseRunController(
            ILogger<CopyCourseRunController> logger,
            ICourseService courseService,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderContextProvider providerContextProvider,
            ICurrentUserProvider currentUserProvider,
            IClock clock,
            IRegionCache regionCache)
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
            _currentUserProvider = currentUserProvider;
            _clock = clock;
            _regionCache = regionCache;
        }

        [Authorize]
        public IActionResult AddNewVenue(CourseRunRequestModel model)
        {
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

            return Json(new Url(Url.Action("Index", "AddVenue", new { returnUrl = Url.Action("Reload", "CopyCourseRun") }))
                .WithProviderContext(_providerContextProvider.GetProviderContext(withLegacyFallback: true))
                .ToString());
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
            var course = await _sqlQueryDispatcher.ExecuteQuery(new GetCourse() { CourseId = cachedData.CourseId.Value });
            var courseRun = course.CourseRuns.SingleOrDefault(cr => cr.CourseRunId == cachedData.CourseRunId);
            var venues = await GetVenuesForProvider();

            var regions = _courseService.GetRegions();

            foreach (var subRegion in regions.RegionItems.SelectMany(r => r.SubRegion))
            {
                subRegion.Checked = courseRun.SubRegionIds?.Contains(subRegion.Id);
            }

            CopyCourseRunViewModel vm = new CopyCourseRunViewModel
            {
                AwardOrgCode = course.AwardOrgCode,
                LearnAimRef = course.LearnAimRef,
                LearnAimRefTitle = course.LearnAimRefTitle,

                //Mode = mode,
                CourseId = course.CourseId,
                CourseRunId = courseRun.CourseRunId,
                CourseName = courseRun?.CourseName,
                Venues = venues.VenueItems.Select(v => new SelectListItem { Text = v.VenueName, Value = v.Id }).ToList(),
                VenueId = courseRun.VenueId ?? null,
                ChooseRegion = new ChooseRegionModel
                {
                    National = courseRun.National,
                    Regions = regions,
                },
                DeliveryMode = courseRun.DeliveryMode,
                CourseProviderReference = courseRun?.ProviderCourseId,
                DurationUnit = courseRun.DurationUnit,
                DurationLength = courseRun?.DurationValue?.ToString(),
                StartDateType = courseRun.FlexibleStartDate
                    ? StartDateType.FlexibleStartDate
                    : StartDateType.SpecifiedStartDate,
                Day = courseRun.StartDate?.Day.ToString("00"),
                Month = courseRun.StartDate?.Month.ToString("00"),
                Year = courseRun.StartDate?.Year.ToString("0000"),
                StudyMode = courseRun.StudyMode,
                Url = courseRun.CourseWebsite,
                Cost = courseRun.Cost?.ToString("F"),
                CostDescription = courseRun.CostDescription,
                AttendanceMode = courseRun.AttendancePattern,
                QualificationType = course.LearnAimRefTypeDesc,
                NotionalNVQLevelv2 = course.NotionalNVQLevelv2
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(Guid? courseId, Guid? courseRunId)
        {
            if (!_session.GetInt32("UKPRN").HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var ukprn = _session.GetInt32("UKPRN").Value;
            var providerId = _providerContextProvider.GetProviderId(withLegacyFallback: true);

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
                return NotFound();
            }

            var course = await _sqlQueryDispatcher.ExecuteQuery(new GetCourse() { CourseId = courseId.Value });

            if (course == null)
            {
                return NotFound();
            }

            var courseRun = course.CourseRuns.SingleOrDefault(cr => cr.CourseRunId == courseRunId);

            if (courseRun == null)
            {
                return NotFound();
            }

            var venues = await _sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = providerId });
            var regions = _courseService.GetRegions();

            foreach (var subRegion in regions.RegionItems.SelectMany(r => r.SubRegion))
            {
                subRegion.Checked = courseRun.SubRegionIds?.Contains(subRegion.Id);
            }

            var vm = new CopyCourseRunViewModel
            {
                AwardOrgCode = course.AwardOrgCode,
                LearnAimRef = course.LearnAimRef,
                LearnAimRefTitle = course.LearnAimRefTitle,

                CourseId = courseId.Value,
                CourseRunId = courseRunId.Value,
                CourseName = courseRun?.CourseName,
                Venues = venues.Select(v => new SelectListItem { Text = v.VenueName, Value = v.VenueId.ToString() }).ToList(),
                VenueId = courseRun.VenueId ?? Guid.Empty,
                ChooseRegion = new ChooseRegionModel
                {
                    National = courseRun.National,
                    Regions = regions
                },
                DeliveryMode = courseRun.DeliveryMode,
                CourseProviderReference = courseRun?.ProviderCourseId,
                DurationUnit = courseRun.DurationUnit,
                DurationLength = courseRun?.DurationValue?.ToString(),
                StartDateType = courseRun.FlexibleStartDate
                    ? StartDateType.FlexibleStartDate
                    : StartDateType.SpecifiedStartDate,
                Day = courseRun.StartDate?.Day.ToString("00"),
                Month = courseRun.StartDate?.Month.ToString("00"),
                Year = courseRun.StartDate?.Year.ToString("0000"),
                StudyMode = courseRun.StudyMode,
                Url = courseRun.CourseWebsite,
                Cost = courseRun.Cost?.ToString("F"),
                CostDescription = courseRun.CostDescription,
                AttendanceMode = courseRun.AttendancePattern,
                QualificationType = course.LearnAimRefTypeDesc,
                NotionalNVQLevelv2 = course.NotionalNVQLevelv2,
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

                if (savedModel.DeliveryMode == CourseDeliveryMode.ClassroomBased)
                {
                    foreach (var venue in vm.Venues)
                    {
                        venue.Selected = venue.Value == savedModel.VenueId.ToString();
                    }
                }

                if (savedModel.DeliveryMode == CourseDeliveryMode.WorkBased && savedModel.National == false)
                {
                    foreach (var subRegion in regions.RegionItems.SelectMany(r => r.SubRegion))
                    {
                        subRegion.Checked = savedModel.SelectedRegions?.Contains(subRegion.Id);
                    }
                }
            }

            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) + "find-a-course/search";

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
                return NotFound();
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
                return NotFound();
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
                Venues = model.DeliveryMode == CourseDeliveryMode.ClassroomBased
                    ? (await GetVenuesForProvider()).VenueItems
                        .Where(v => v.Id == model.VenueId.ToString())
                        .Select(v => v.VenueName)
                    : Enumerable.Empty<string>(),
                Regions = model.DeliveryMode == CourseDeliveryMode.WorkBased
                    ? model.National == true
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
                return NotFound();
            }

            if (!model.CourseId.HasValue)
            {
                return NotFound();
            }

            var allRegions = await _regionCache.GetAllRegions();
            var allSubRegionIds = allRegions.SelectMany(r => r.SubRegions).Select(sr => sr.Id).ToHashSet();

            bool flexibleStartDate = model.StartDateType != StartDateType.SpecifiedStartDate;
            DateTime? specifiedStartDate = null;

            if (!flexibleStartDate)
            {
                specifiedStartDate = DateTime.ParseExact(
                    $"{int.Parse(model.Day):00}-{int.Parse(model.Month):00}-{model.Year}",
                    "dd-MM-yyyy",
                    System.Globalization.CultureInfo.InvariantCulture);
            }

            if (model.National == true)
            {
                model.SelectedRegions = null;
            }

            var validationResult = new CopyCourseRunSaveViewModelValidator(allRegions, _clock).Validate(model);
            if (!validationResult.IsValid)
            {
                return BadRequest();
            }

            var createCommand = new CreateCourseRun()
            {
                CourseId = model.CourseId.Value,
                CourseRunId = Guid.NewGuid(),
                DurationUnit = model.DurationUnit,
                DeliveryMode = model.DeliveryMode,
                Cost = !string.IsNullOrEmpty(model.Cost) ? Convert.ToDecimal(model.Cost) : (decimal?)null,
                CostDescription = model.CostDescription ?? "",
                CourseName = model.CourseName,
                CourseUrl = model.Url,
                DurationValue = Convert.ToInt32(model.DurationLength),
                ProviderCourseId = model.CourseProviderReference ?? "",
                FlexibleStartDate = flexibleStartDate,
                StartDate = specifiedStartDate,
                CreatedBy = _currentUserProvider.GetCurrentUser(),
                CreatedOn = _clock.UtcNow
            };

            switch (model.DeliveryMode)
            {
                case CourseDeliveryMode.ClassroomBased:
                    createCommand.AttendancePattern = model.AttendanceMode;
                    createCommand.StudyMode = model.StudyMode;

                    createCommand.SubRegionIds = null;
                    createCommand.VenueId = model.VenueId;
                    break;
                case CourseDeliveryMode.WorkBased:
                    createCommand.VenueId = null;

                    var availableRegions = new SelectRegionModel();

                    if (model.National == true)
                    {
                        createCommand.National = true;
                        createCommand.SubRegionIds = availableRegions.RegionItems.Select(x => x.Id).ToList();
                    }
                    else
                    {
                        createCommand.National = false;
                        createCommand.SubRegionIds = model.SelectedRegions.Where(id => allSubRegionIds.Contains(id));

                        createCommand.AttendancePattern = null;
                        createCommand.StudyMode = null;
                    }
                    break;
                case CourseDeliveryMode.Online:

                    createCommand.SubRegionIds = null;
                    createCommand.VenueId = null;
                    createCommand.AttendancePattern = null;
                    createCommand.StudyMode = null;
                    break;
            }

            var createResult = await _sqlQueryDispatcher.ExecuteQuery(createCommand);

            if (!(createResult.Value is Success))
            {
                return BadRequest();
            }

            _session.SetObject(CopyCourseRunPublishedCourseSessionKey, new PublishedCourseViewModel
            {
                CourseId = createCommand.CourseId,
                CourseRunId = createCommand.CourseRunId,
                CourseName = createCommand.CourseName
            });

            _session.Remove("NewAddedVenue");
            _session.Remove("Option");
            _session.Remove(CopyCourseRunSaveViewModelSessionKey);

            return RedirectToAction("Published");
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

            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) + 
                "find-a-course/course-details?CourseId=" + publishedCourse.CourseId + "&r=" + publishedCourse.CourseRunId; 

            return View(publishedCourse);
        }

        private class CopyCourseRunSaveViewModelValidator : AbstractValidator<CopyCourseRunSaveViewModel>
        {
            public CopyCourseRunSaveViewModelValidator(IReadOnlyCollection<Region> allRegions, IClock clock)
            {
                RuleFor(c => c.AttendanceMode)
                    .AttendancePattern(attendancePatternWasSpecified: c => c.AttendanceMode.HasValue, getDeliveryMode: c => c.DeliveryMode);

                RuleFor(c => c.Cost)
                    .Transform(v => decimal.TryParse(v, out var d) ? d : (decimal?)null)
                    .Cost(costWasSpecified: c => !string.IsNullOrEmpty(c.Cost), getCostDescription: c => c.CostDescription);

                RuleFor(c => c.CostDescription)
                    .CostDescription();

                RuleFor(c => c.CourseName).CourseName();

                RuleFor(c => c.CourseProviderReference).ProviderCourseRef();

                RuleFor(c => c.DeliveryMode).IsInEnum();

                RuleFor(c => c.DurationLength)
                    .Transform(v => int.TryParse(v, out var i) ? i : (int?)i)
                    .Duration();

                RuleFor(c => c.DurationUnit).IsInEnum();

                RuleFor(c => c.National)
                    .Transform(v => (bool?)v)
                    .NationalDelivery(getDeliveryMode: c => c.DeliveryMode);

                RuleFor(c => c.SelectedRegions)
                    .Transform(v =>
                    {
                        if (v == null)
                        {
                            return null;
                        }

                        var allSubRegions = allRegions.SelectMany(r => r.SubRegions).ToDictionary(sr => sr.Id, sr => sr);
                        return v.Select(id => allSubRegions[id]).ToArray();
                    })
                    .SubRegions(subRegionsWereSpecified: c => c.SelectedRegions?.Count() > 0, getDeliveryMode: c => c.DeliveryMode, getNationalDelivery: c => c.National);

                RuleFor(c => c.StudyMode)
                    .StudyMode(studyModeWasSpecified: c => c.StudyMode.HasValue, getDeliveryMode: c => c.DeliveryMode);

                RuleFor(c => c.Url).CourseWebPage();

                RuleFor(c => c.VenueId)
                    .Transform(v => v == default ? (Guid?)null : v)
                    .VenueId(getDeliveryMode: c => c.DeliveryMode);
            }
        }
    }
}
