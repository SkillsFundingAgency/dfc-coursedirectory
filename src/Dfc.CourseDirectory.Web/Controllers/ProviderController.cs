using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Helpers;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Models.Models.Regions;
using Dfc.CourseDirectory.Models.Models.Venues;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.Provider;
using Dfc.CourseDirectory.Web.ViewModels.YourCourses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.Helpers.Attributes;
using Dfc.CourseDirectory.WebV2;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class ProviderController : Controller
    {
        private readonly ILogger<ProviderController> _logger;
        private readonly ISession _session;
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;
        private readonly IProviderService _providerService;
        private readonly IAuthorizationService _authorizationService;

        public ProviderController(
            ILogger<ProviderController> logger,
            IHttpContextAccessor contextAccessor,
            ICourseService courseService,
            IVenueService venueService,
            IProviderService providerService,
            IAuthorizationService Authorization, IAuthorizationService authorizationService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(venueService, nameof(venueService));
            Throw.IfNull(providerService, nameof(providerService));

            _logger = logger;
            _session = contextAccessor.HttpContext.Session;
            _courseService = courseService;
            _venueService = venueService;
            _providerService = providerService;
            _authorizationService = authorizationService;
        }

        [Authorize(Policy = "ElevatedUserRole")]
        public IActionResult Index()
        {
            _session.SetString("Option", "Provider");
            _session.SetInt32("ProviderSearch", 0);
            _logger.LogMethodEnter();
            _logger.LogMethodExit();
            return View();
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

        internal string CourseNameByCourseRunId(IEnumerable<CourseRunViewModel> courseRuns, string id)
        {
            if (courseRuns == null) return string.Empty;
            if (string.IsNullOrWhiteSpace(id)) return string.Empty;

            var found = courseRuns.FirstOrDefault(x => x.Id == id)?.CourseName;

            return found;
        }

        internal string QualificationTitleByCourseId(IEnumerable<CourseViewModel> courses, string id)
        {
            if (courses == null) return string.Empty;
            if (string.IsNullOrWhiteSpace(id)) return string.Empty;

            var found = courses.FirstOrDefault(x => x.Id == id)?.QualificationTitle;

            return found;
        }

        [Authorize("Admin")]
        [SelectedProviderNeeded]
        public async Task<IActionResult> AddOrEditProviderType()
        {
            var model = new ProviderTypeAddOrEditViewModel();

            int? UKPRN = _session.GetInt32("UKPRN");

            var providerSearchResult = await _providerService.GetProviderByPRNAsync(new Dfc.CourseDirectory.Services.ProviderService.ProviderSearchCriteria(UKPRN.ToString()));

            if (providerSearchResult.IsSuccess)
            {
                model = new ProviderTypeAddOrEditViewModel();
                model.ProviderType = providerSearchResult.Value.Value.FirstOrDefault()?.ProviderType ?? ProviderType.Undefined;
            }

            return View("UpdateProviderType", model);
        }

        [Authorize("Admin")]
        [HttpPost]
        [SelectedProviderNeeded]
        public async Task<IActionResult> AddOrEditProviderType(
            ProviderTypeAddOrEditViewModel model,
            [FromServices] IProviderInfoCache providerInfoCache,
            [FromServices] ISqlQueryDispatcher sqlQueryDispatcher)
        {
            int? UKPRN = _session.GetInt32("UKPRN");

            var providerSearchResult = await _providerService.GetProviderByPRNAsync(new Services.ProviderService.ProviderSearchCriteria(UKPRN.ToString()));

            if (providerSearchResult.IsSuccess)
            {

                Provider provider = providerSearchResult.Value.Value.FirstOrDefault();

                var oldProviderType = provider.ProviderType;
                if (oldProviderType == ProviderType.FE && model.ProviderType.HasFlag(ProviderType.Apprenticeship))
                {
                    var providerId = await providerInfoCache.GetProviderIdForUkprn(UKPRN.Value);
                    await sqlQueryDispatcher.ExecuteQuery(
                        new EnsureApprenticeshipQAStatusSetForProvider()
                        {
                            ProviderId = providerId.Value
                        });
                }

                provider.ProviderType = model.ProviderType;

                try
                {
                    var result = await _providerService.UpdateProviderDetails(provider);
                    if (result.IsSuccess)
                    {
                        //log something
                    }

                }
                catch (Exception)
                {
                    //Behaviour to be determined for failure
                }

            }

            return RedirectToAction("Details", "Provider", new { });
        }

       


        [Authorize]
        [SelectedProviderNeeded]
        public async Task<IActionResult> Details()
        {

            var model = new ProviderDetailsViewModel();

            int? UKPRN = _session.GetInt32("UKPRN");

            var providerSearchResult = await _providerService.GetProviderByPRNAsync(new Dfc.CourseDirectory.Services.ProviderService.ProviderSearchCriteria(UKPRN.ToString()));

            if (providerSearchResult.IsSuccess)
            {
                var provider = providerSearchResult.Value.Value.FirstOrDefault();

                model.Status = provider.ProviderStatus;
                model.ProviderName = string.IsNullOrEmpty(provider.CourseDirectoryName) ? provider.ProviderName: provider.CourseDirectoryName;
                model.LegalName = provider.ProviderName;

                if (provider.ProviderAliases != null)
                {
                    model.TradingName = provider.ProviderAliases?.FirstOrDefault()?.ProviderAlias?.ToString();
                    model.AliasName = provider.ProviderAliases.FirstOrDefault()?.ProviderAlias?.ToString();
                }
              
                model.UKPRN = UKPRN.ToString();
                model.UnitedKingdomProviderReferenceNumber = provider.UnitedKingdomProviderReferenceNumber;
                model.BriefOverview = provider.MarketingInformation;
                model.Alias = provider.Alias;
                model.ProviderType = provider.ProviderType;

                string AddressLine1 = string.Empty;
                string AddressLine2 = string.Empty;
                string AddressLine3 = string.Empty;
                string AddressLine4 = string.Empty;
                string TownCity = string.Empty;
                string PostCode = string.Empty;

                if (provider.ProviderContact != null)
                {
                    var providerContactTypeP = provider.ProviderContact.Where(s => s.ContactType.Equals("P", StringComparison.InvariantCultureIgnoreCase));
                    if (providerContactTypeP.FirstOrDefault()?.ContactAddress?.SAON?.Description != null)
                    {
                        AddressLine1 = providerContactTypeP.FirstOrDefault()?.ContactAddress?.SAON?.Description.ToString();
                    }

                    if (providerContactTypeP.FirstOrDefault()?.ContactAddress?.PAON?.Description != null)
                    {
                        AddressLine2 = providerContactTypeP.FirstOrDefault()?.ContactAddress?.PAON?.Description.ToString();
                    }


                    if (providerContactTypeP.FirstOrDefault()?.ContactAddress?.StreetDescription != null)
                    {
                        AddressLine3 = providerContactTypeP.FirstOrDefault()?.ContactAddress?.StreetDescription.ToString();
                    }


                    if (providerContactTypeP.FirstOrDefault()?.ContactAddress?.Locality != null)
                    {
                        AddressLine4 = providerContactTypeP.FirstOrDefault()?.ContactAddress?.Locality.ToString();
                    }


                    if (!string.IsNullOrEmpty(providerContactTypeP.FirstOrDefault()?.ContactAddress?.Items?.FirstOrDefault()))
                    {
                        TownCity = providerContactTypeP.FirstOrDefault()?.ContactAddress?.Items?.FirstOrDefault();
                    }


                    if (!string.IsNullOrEmpty(providerContactTypeP?.FirstOrDefault()?.ContactAddress?.PostCode))
                    {
                        PostCode = providerContactTypeP?.FirstOrDefault()?.ContactAddress?.PostCode;
                    }


                    model.AddressLine1 = AddressLine1;
                    model.AddressLine2 = AddressLine2;
                    model.AddressLine3 = AddressLine3;
                    model.AddressLine4 = AddressLine4;
                    model.TownCity = TownCity;

                    model.PostCode = PostCode;

                    model.Telephone = providerContactTypeP?.FirstOrDefault()?.ContactTelephone1;
                    model.Web = providerContactTypeP?.FirstOrDefault()?.ContactWebsiteAddress;
                    model.Email = providerContactTypeP?.FirstOrDefault()?.ContactEmail;
                }

            }




            return View(model);
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }

        [Authorize]
        [SelectedProviderNeeded]
        public async Task<IActionResult> Courses(
            string level,
            Guid? courseId,
            Guid? courseRunId,
            string notificationTitle,
            string notificationMessage)
        {

            _session.SetString("Option", "Courses");
            int? UKPRN = _session.GetInt32("UKPRN");

            var courseResult = (await _courseService.GetCoursesByLevelForUKPRNAsync(new CourseSearchCriteria(UKPRN))).Value;
            var venueResult = (await _venueService.SearchAsync(new VenueSearchCriteria(UKPRN.ToString(), string.Empty))).Value;
            var allRegions = _courseService.GetRegions().RegionItems;


            var Courses = courseResult.Value.SelectMany(o => o.Value).SelectMany(i => i.Value).ToList();

            var filteredCourses = from Course c in Courses.Where(c => BitmaskHelper.IsSet(c.CourseStatus, RecordStatus.Live)).ToList().OrderBy(x => x.QualificationCourseTitle)
                                  select c;

            var pendingCourses = from Course c in Courses.Where(c => c.CourseStatus== RecordStatus.MigrationPending || c.CourseStatus== RecordStatus.BulkUploadPending)
                                  select c;

            foreach (var course in filteredCourses)
            {
                var filteredCourseRuns = new List<CourseRun>();

                filteredCourseRuns = course.CourseRuns.ToList();
                filteredCourseRuns.RemoveAll(x => x.RecordStatus != RecordStatus.Live);

                course.CourseRuns = filteredCourseRuns;
            }

            var levelFilters = filteredCourses.GroupBy(x => x.NotionalNVQLevelv2).OrderBy(x => x.Key).ToList();

            var levelFiltersForDisplay = new List<QualificationLevelFilterViewModel>();

            var courseViewModels = new List<CourseViewModel>();

            if (string.IsNullOrWhiteSpace(level))
            {
                level = levelFilters.FirstOrDefault()?.Key;
            }
            else
            {
                if (!filteredCourses.Any(x => x.NotionalNVQLevelv2 == level))
                {
                    level = levelFilters.FirstOrDefault()?.Key;
                }
            }

            foreach (var levels in levelFilters)
            {

                var lf = new QualificationLevelFilterViewModel()
                {
                    Facet = levels.ToList().Count().ToString(),
                    IsSelected = level == levels.Key,
                    Value = levels.Key,
                    Name = $"Level {levels.Key}",

                };

                levelFiltersForDisplay.Add(lf);

                foreach (var course in levels)
                {
                    if (course.NotionalNVQLevelv2 == level)
                    {
                        var courseVM = new CourseViewModel()
                        {
                            Id = course.id.ToString(),
                            AwardOrg = course.AwardOrgCode,
                            LearnAimRef = course.LearnAimRef,
                            NotionalNVQLevelv2 = course.NotionalNVQLevelv2,
                            QualificationTitle = course.QualificationCourseTitle,
                            QualificationType = course.QualificationType,
                            Facet = course.CourseRuns.Count().ToString(),
                            CourseRuns = course.CourseRuns.Select(y => new CourseRunViewModel
                            {
                                Id = y.id.ToString(),
                                CourseId = y.ProviderCourseID,
                                AttendancePattern = y.AttendancePattern.ToDescription(),
                                Cost = y.Cost.HasValue ? $"£ {y.Cost.Value:0.00}" : string.Empty,
                                CourseName = y.CourseName,
                                DeliveryMode = y.DeliveryMode.ToDescription(),
                                Duration = y.DurationValue.HasValue
                                        ? $"{y.DurationValue.Value} {y.DurationUnit.ToDescription()}"
                                        : $"0 {y.DurationUnit.ToDescription()}",
                                Venue = y.VenueId.HasValue
                                        ? FormatAddress(GetVenueByIdFrom(venueResult.Value, y.VenueId.Value))
                                        : string.Empty,
                                Region = y.Regions != null
                                        ? FormattedRegionsByIds(allRegions, y.Regions)
                                        : string.Empty,
                                StartDate = y.FlexibleStartDate
                                        ? "Flexible start date"
                                        : y.StartDate?.ToString("dd/MM/yyyy"),
                                StudyMode = y.StudyMode == Models.Models.Courses.StudyMode.Undefined
                                        ? string.Empty
                                        : y.StudyMode.ToDescription(),
                                Url = y.CourseURL
                            })
                                .OrderBy(y => y.CourseName)
                                .ToList()
                        };

                        courseViewModels.Add(courseVM);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(level))
            {
                level = levelFiltersForDisplay.FirstOrDefault()?.Value;
                levelFiltersForDisplay.ForEach(x => { if (x.Value == level) { x.IsSelected = true; } });
            }

            courseViewModels.OrderBy(x => x.QualificationTitle).ToList();

            var notificationCourseName = string.Empty;
            var notificationAnchorTag = string.Empty;

            if (courseId.HasValue && courseId.Value != Guid.Empty)
            {
                notificationCourseName = QualificationTitleByCourseId(courseViewModels, courseId.ToString());

                if (courseRunId.HasValue && courseRunId.Value != Guid.Empty)
                {
                    var courseRuns = courseViewModels.Find(x => x.Id == courseId.Value.ToString())?.CourseRuns;
                    notificationCourseName = CourseNameByCourseRunId(courseRuns, courseRunId.ToString());
                }
            }

            if (!courseRunId.HasValue)
            {
                notificationMessage = string.Empty;
            }

            notificationCourseName = Regex.Replace(notificationCourseName, "<.*?>", String.Empty);
            notificationAnchorTag = courseRunId.HasValue
                ? $"<a id=\"courseeditlink\" class=\"govuk-link\" href=\"#\" data-courseid=\"{courseId}\" data-courserunid=\"{courseRunId}\" >{notificationMessage} {notificationCourseName}</a>"
                : $"<a id=\"courseeditlink\" class=\"govuk-link\" href=\"#\" data-courseid=\"{courseId}\">{notificationMessage} {notificationCourseName}</a>";

            var viewModel = new YourCoursesViewModel
            {
                Heading = $"Level {level}",
                HeadingCaption = "Your courses",
                Courses = courseViewModels ?? new List<CourseViewModel>(),
                LevelFilters = levelFiltersForDisplay,
                NotificationTitle = notificationTitle,
                NotificationMessage = notificationAnchorTag,
                PendingCoursesCount = pendingCourses?.Count()
            };

            return View(viewModel);
        }
    }
}