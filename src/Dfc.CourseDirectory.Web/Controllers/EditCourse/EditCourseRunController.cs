using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.BinaryStorageProvider;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Helpers;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Services;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Services.Models.Regions;
using Dfc.CourseDirectory.Web.Extensions;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.Validation;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseRegion;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.SelectVenue;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.EditCourse;
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

namespace Dfc.CourseDirectory.Web.Controllers.EditCourse
{
    public class EditCourseRunController : BaseController
    {
        private readonly ICourseService _courseService;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private const string NonLars = "NonLars";

        private ISession Session => HttpContext.Session;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;
        private readonly IRegionCache _regionCache;
        private readonly ILogger<BlobStorageBinaryStorageProvider> _log;
        private readonly IWebRiskService _webRiskService;

        public EditCourseRunController(
            ICourseService courseService,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderContextProvider providerContextProvider,
            ICurrentUserProvider currentUserProvider,
            IClock clock,
            IRegionCache regionCache,
            ILogger<BlobStorageBinaryStorageProvider> log,
            IWebRiskService webRiskService) : base(sqlQueryDispatcher)
        {
            if (courseService == null)
            {
                throw new ArgumentNullException(nameof(courseService));
            }

            _courseService = courseService;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerContextProvider = providerContextProvider;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
            _regionCache = regionCache;
            _log = log;
            _webRiskService = webRiskService;
        }

        [Authorize]
        public IActionResult AddNewVenue(CourseRunRequestModel model)
        {
            _log.LogInformation("EditCourseRunController AddNewVenue");

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

            var returnurl = new Url(Url.Action("Index", "AddVenue", new { returnUrl = Url.Action("Reload", "EditCourseRun") }))
                .WithProviderContext(_providerContextProvider.GetProviderContext(withLegacyFallback: true))
                .ToString();

            _log.LogInformation("EditCourseRunController AddNewVenue [{returnurl}]", returnurl);

            return Json(returnurl);
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

            var course = await GetCourse(cachedData.CourseId.Value);

            var courseRun = course.CourseRuns.SingleOrDefault(cr => cr.CourseRunId == cachedData.CourseRunId);


            EditCourseRunViewModel vm = new EditCourseRunViewModel
            {
                AwardOrgCode = cachedData.AwardOrgCode,
                LearnAimRef = cachedData.LearnAimRef,
                LearnAimRefTitle = cachedData.LearnAimRefTitle,

                //Mode = mode,
                CourseId = course.CourseId,
                CourseRunId = courseRun.CourseRunId,
                CourseName = courseRun?.CourseName,
                Venues = courseRunVenues,
                VenueId = courseRun.VenueId ?? (Guid?)null,
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
                CurrentCourseRunDate = courseRun.StartDate
            };

            vm.ValPastDateRef = DateTime.Now;
            vm.ValPastDateMessage = "Start Date cannot be earlier than today’s date";

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

            if ((courseRun.SubRegionIds?.Count ?? 0) == 0) return View("EditCourseRun", vm);

            foreach (var selectRegionRegionItem in vm.ChooseRegion.Regions.RegionItems.OrderBy(x => x.RegionName))
            {
                foreach (var subRegionItemModel in selectRegionRegionItem.SubRegion)
                {
                    if (courseRun.SubRegionIds.Contains(subRegionItemModel.Id))
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

            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) + "find-a-course/search";

            return View("EditCourseRun", vm);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(string learnAimRef,
            string notionalNVQLevelv2,
            string awardOrgCode,
            string learnAimRefTitle,
            string learnAimRefTypeDesc,
            Guid? courseId,
            Guid courseRunId,
            PublishMode mode,
            string type)
        {
            int? UKPRN;

            _log.LogInformation("EditCourseRunController Index");
            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) + "find-a-course/search";

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
            };

            var regions = _courseService.GetRegions();

            Session.SetObject(SessionVenues, venues);
            Session.SetObject(SessionRegions, regions);

            SetLarsSession(type);

            if (courseId.HasValue)
            {
                var nonLarsCourse = IsCourseNonLars();
                var course = await GetCourse(courseId, nonLarsCourse);

                var courseRun = course.CourseRuns.SingleOrDefault(cr => cr.CourseRunId == courseRunId);

                if (courseRun != null)
                {
                    _log.LogInformation("EditCourseRunController Index courseRun !=null");

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
                            National = courseRun.DeliveryMode != CourseDeliveryMode.WorkBased ? null : courseRun.National,
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
                        CurrentCourseRunDate = courseRun.StartDate,
                        NonLarsCourse = nonLarsCourse,
                        CourseType = course.CourseType,
                        SectorId = course.SectorId,
                        Sectors = await GetSectors(),
                        EducationLevel = course.EducationLevel,
                        AwardingBody = course.AwardingBody
                    };

                    vm.ValPastDateRef = DateTime.Now;
                    vm.ValPastDateMessage = "Start Date cannot be earlier than today’s date";

                    if ((courseRun.SubRegionIds?.Count ?? 0) == 0) return View("EditCourseRun", vm);

                    foreach (var selectRegionRegionItem in vm.ChooseRegion.Regions.RegionItems.OrderBy(x => x.RegionName))
                    {
                        //If Region is returned, check for existence of any subregions
                        if (courseRun.SubRegionIds.Contains(selectRegionRegionItem.Id))
                        {
                            var subregionsInList = from subRegion in selectRegionRegionItem.SubRegion
                                                   where courseRun.SubRegionIds.Contains(subRegion.Id)
                                                   select subRegion;

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
                                if (courseRun.SubRegionIds.Contains(subRegionItemModel.Id))
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
                    _log.LogInformation("EditCourseRunController Index EditCourseRun");
                    return View("EditCourseRun", vm);
                }
            }
            _log.LogInformation("EditCourseRunController Index error {Error}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
            //error page
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Index(EditCourseRunSaveViewModel model)
        {
            if (!model.CourseId.HasValue)
            {
                return BadRequest();
            }
            model.NonLarsCourse = IsCourseNonLars();
            var courseId = model.CourseId.Value;

            var allRegions = await _regionCache.GetAllRegions();
            var allSubRegionIds = allRegions.SelectMany(r => r.SubRegions).Select(sr => sr.Id).ToHashSet();

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

                model.StartDate = specifiedStartDate.Value;

                flexibleStartDate = false;
            }

            if (model.National == true)
            {
                model.SelectedRegions = null;
            }

            model.FlexibleStartDate = flexibleStartDate;

            var validationResult = await new EditCourseRunSaveViewModelValidator(allRegions, _clock, _webRiskService).ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                return BadRequest();
            }

            var course = await GetCourse(courseId);
            var updateCourseResult = await _sqlQueryDispatcher.ExecuteQuery(new UpdateCourse()
            {
                CourseId = courseId,
                WhoThisCourseIsFor = course.CourseDescription,
                EntryRequirements = course.EntryRequirements,
                WhatYoullLearn = course.WhatYoullLearn,
                HowYoullLearn = course.HowYoullLearn,
                WhatYoullNeed = course.WhatYoullNeed,
                HowYoullBeAssessed = course.HowYoullBeAssessed,
                WhereNext = course.WhereNext,
                UpdatedBy = _currentUserProvider.GetCurrentUser(),
                UpdatedOn = _clock.UtcNow,
                CourseType = course.CourseType,
                SectorId = model.SectorId,
                EducationLevel = model.EducationLevel,
                AwardingBody = model.AwardingBody
            });

            var updateCommand = new UpdateCourseRun()
            {
                CourseRunId = model.CourseRunId,
                DurationUnit = model.DurationUnit,
                DeliveryMode = model.DeliveryMode,
                Cost = !string.IsNullOrEmpty(model.Cost) ? Convert.ToDecimal(model.Cost) : (decimal?)null,
                CostDescription = ASCIICodeHelper.ReplaceHexCodes(model.CostDescription) ?? "",
                CourseName = model.CourseName,
                CourseUrl = model.Url,
                DurationValue = Convert.ToInt32(model.DurationLength),
                ProviderCourseId = model.CourseProviderReference ?? "",
                FlexibleStartDate = flexibleStartDate,
                StartDate = specifiedStartDate,
                UpdatedBy = _currentUserProvider.GetCurrentUser(),
                UpdatedOn = _clock.UtcNow
            };

            updateCommand.National = null;
            switch (model.DeliveryMode)
            {
                case CourseDeliveryMode.BlendedLearning:
                case CourseDeliveryMode.ClassroomBased:
                    updateCommand.National = null;
                    updateCommand.SubRegionIds = null;
                    updateCommand.VenueId = model.VenueId;

                    updateCommand.AttendancePattern = model.AttendanceMode;
                    updateCommand.StudyMode = model.StudyMode;

                    break;
                case CourseDeliveryMode.WorkBased:
                    updateCommand.VenueId = null;
                    var availableRegions = new SelectRegionModel();
                    if (model.National.Value)
                    {
                        updateCommand.National = true;
                        updateCommand.SubRegionIds = null;
                    }
                    else
                    {
                        updateCommand.National = false;
                        updateCommand.SubRegionIds = model.SelectedRegions.Where(id => allSubRegionIds.Contains(id));
                        updateCommand.AttendancePattern = null;
                        updateCommand.StudyMode = null;
                    }
                    break;
                case CourseDeliveryMode.Online:

                    updateCommand.SubRegionIds = null;
                    updateCommand.VenueId = null;
                    updateCommand.National = null;
                    updateCommand.AttendancePattern = null;
                    updateCommand.StudyMode = null;

                    break;
            }

            var updateResult = await _sqlQueryDispatcher.ExecuteQuery(updateCommand);

            if (!(updateResult.Value is Success))
            {
                return BadRequest();
            }

            Session.Remove("NewAddedVenue");
            Session.Remove("Option");

            switch (model.Mode)
            {
                case PublishMode.DataQualityIndicator:
                    TempData[TempDataKeys.ExpiredCoursesNotification] = model.CourseName + " has been updated";
                    return RedirectToAction("Index", "ExpiredCourseRuns", new { isnonlars = model.NonLarsCourse })
                        .WithProviderContext(_providerContextProvider.GetProviderContext(withLegacyFallback: true));
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

        private void SetLarsSession(string type)
        {
            if (type == NonLars)
            {
                Session.SetString(SessionNonLarsCourse, "true");
            }
            else
            {
                Session.SetString(SessionNonLarsCourse, "false");
            }
        }
    }
}
