﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Regions;
using Dfc.CourseDirectory.Web.Configuration;
using Dfc.CourseDirectory.Web.Extensions;
using Dfc.CourseDirectory.Web.Helpers.Attributes;
using Dfc.CourseDirectory.Web.Models.Apprenticeships;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships.ApprenticeshipSearchResult;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseRegion;
using Dfc.CourseDirectory.Web.ViewModels.Apprenticeships;
using Dfc.CourseDirectory.WebV2;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Security;
using Flurl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OneOf.Types;

namespace Dfc.CourseDirectory.Web.Controllers
{
    [Authorize("Apprenticeship")]
    [SelectedProviderNeeded]
    [RestrictApprenticeshipQAStatus(ApprenticeshipQAStatus.Passed)]
    public class ApprenticeshipsController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IOptions<ApprenticeshipSettings> _apprenticeshipSettings;
        private readonly IStandardsCache _standardsAndFrameworksCache;
        private readonly IProviderInfoCache _providerInfoCache;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly IFeatureFlagProvider _featureFlagProvider;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;

        private ISession _session => HttpContext.Session;

        public ApprenticeshipsController(
            ICourseService courseService,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IOptions<ApprenticeshipSettings> apprenticeshipSettings,
            IStandardsCache standardsAndFrameworksCache,
            IProviderInfoCache providerInfoCache,
            IProviderContextProvider providerContextProvider,
            IFeatureFlagProvider featureFlagProvider,
            ICurrentUserProvider currentUserProvider,
            IClock clock)
        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _apprenticeshipSettings = apprenticeshipSettings ?? throw new ArgumentNullException(nameof(apprenticeshipSettings));
            _standardsAndFrameworksCache = standardsAndFrameworksCache ?? throw new ArgumentNullException(nameof(standardsAndFrameworksCache));
            _providerInfoCache = providerInfoCache;
            _providerContextProvider = providerContextProvider;
            _featureFlagProvider = featureFlagProvider;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
        }

        [Authorize]
        public IActionResult Index()
        {
            _session.Remove("selectedApprenticeship");
            return View("../ApprenticeShips/Search/Index");
        }

        [Authorize]
        public async Task<IActionResult> Search([FromQuery] SearchRequestModel requestModel)
        {
            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            if (string.IsNullOrWhiteSpace(requestModel?.SearchTerm))
            {
                return ViewComponent(nameof(ApprenticeshipSearchResult), new ApprenticeshipsSearchResultModel());
            }

            var searchTermWords = GetSearchWords(requestModel.SearchTerm);
            var standards = await _standardsAndFrameworksCache.GetAllStandards();

            // Match whenever any search term is found in reference words using a prefix match
            // i.e. search for 'retail' should match 'retail' & 'retailer'
            // but search for 'etail' should not match either
            // *unless* the search term is a number in which case the entire word must match
            var results = standards.Where(standard =>
            {
                var words = GetSearchWords(standard.StandardName);

                return searchTermWords.Any(searchTermWord =>
                    words.Any(word => searchTermWord.All(char.IsDigit)
                        ? word.Equals(searchTermWord, StringComparison.InvariantCultureIgnoreCase)
                        : word.StartsWith(searchTermWord, StringComparison.InvariantCultureIgnoreCase)));
            });

            return ViewComponent(nameof(ApprenticeshipSearchResult), new ApprenticeshipsSearchResultModel
            {
                SearchTerm = requestModel.SearchTerm,
                Items = results.Select(r => new ApprenticeShipsSearchResultItemModel
                {
                    ApprenticeshipTitle = r.StandardName,
                    StandardCode = r.StandardCode,
                    Version = r.Version,
                    OtherBodyApprovalRequired = r.OtherBodyApprovalRequired,
                    ApprenticeshipType = ApprenticeshipType.Standard,
                    NotionalNVQLevelv2 = r.NotionalNVQLevelv2
                })
            });

            static IReadOnlyCollection<string> GetSearchWords(string searchTerm) =>
                new string(searchTerm.Select(c => char.IsLetterOrDigit(c) ? c : ' ').ToArray())
                    .Split(" ", StringSplitOptions.RemoveEmptyEntries);
        }

        [Authorize]
        public IActionResult Details(DetailsRequestModel request)
        {
            var model = new DetailViewModel();

            var mode = ApprenticeshipMode.Undefined;
            if (request.Mode != ApprenticeshipMode.Undefined)
            {
                _session.SetObject("ApprenticeshipMode", request.Mode);
                mode = request.Mode;
            }
            else
            {
                mode = _session.GetObject<ApprenticeshipMode>("ApprenticeshipMode");
            }

            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");
            if (apprenticeship != null)
            {
                model = MapToDetailViewModel(apprenticeship, mode);
                model.Cancelled = request.Cancelled.HasValue && request.Cancelled.Value;
            }
            else
            {
                model.ApprenticeshipTitle = request.ApprenticeshipTitle;
                model.ApprenticeshipPreviousPage = request.PreviousPage;
                model.StandardCode = request.StandardCode;
                model.Version = request.Version;
                model.NotionalNVQLevelv2 = request.NotionalNVQLevelv2;
                model.Cancelled = request.Cancelled.HasValue && request.Cancelled.Value;
                model.Mode = mode;
            }

            if (request.ShowCancelled.HasValue)
            {
                model.ShowCancelled = request.ShowCancelled.Value;
            }

            return View("../ApprenticeShips/Details/Index", model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Details(DetailViewModel model)
        {
            var ukprn = _session.GetInt32("UKPRN").Value;
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship")
                ?? await MapToApprenticeship(model, ukprn, new List<ApprenticeshipLocation>());

            var mode = _session.GetObject<ApprenticeshipMode>("ApprenticeshipMode");
            model.Mode = mode;

            if (model.Mode == ApprenticeshipMode.Add ||
                model.Mode == ApprenticeshipMode.EditApprenticeship ||
                model.Mode == ApprenticeshipMode.EditYourApprenticeships && apprenticeship != null)
            {
                apprenticeship.ApprenticeshipTitle = model.ApprenticeshipTitle;
                apprenticeship.ApprenticeshipType = model.ApprenticeshipType;
                apprenticeship.StandardCode = model.StandardCode;
                apprenticeship.MarketingInformation = model.Information;
                apprenticeship.Url = model.Website;
                apprenticeship.ContactTelephone = model.Telephone;
                apprenticeship.ContactEmail = model.Email;
                apprenticeship.ContactWebsite = model.ContactUsIUrl;
                apprenticeship.NotionalNVQLevelv2 = model.NotionalNVQLevelv2;
            }

            _session.SetObject("selectedApprenticeship", apprenticeship);

            switch (model.Mode)
            {
                case ApprenticeshipMode.Add:
                    if (model.ShowCancelled.HasValue && model.ShowCancelled.Value == true)
                    {
                        return RedirectToAction("Summary", "Apprenticeships", new SummaryRequestModel() { SummaryOnly = false });
                    }
                    return RedirectToAction("Delivery", "Apprenticeships", new DeliveryRequestModel() { Mode = model.Mode });
                case ApprenticeshipMode.EditApprenticeship:
                    return RedirectToAction("Summary", "Apprenticeships",
                        new SummaryRequestModel() { SummaryOnly = false });
                case ApprenticeshipMode.EditYourApprenticeships:
                    return RedirectToAction("Summary", "Apprenticeships", new SummaryRequestModel() { SummaryOnly = false });
                default:
                    return RedirectToAction("Delivery", "Apprenticeships", new DeliveryRequestModel() { Mode = model.Mode });
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult Delivery()
        {
            return View("../ApprenticeShips/Delivery/Index", new DeliveryViewModel());
        }

        [Authorize]
        [HttpPost]
        public IActionResult Delivery(DeliveryViewModel model)
        {
            switch (model.ApprenticeshipDelivery)
            {
                case ApprenticeshipDelivery.Both:
                    return RedirectToAction("DeliveryOptionsCombined", "Apprenticeships");
                case ApprenticeshipDelivery.EmployersAddress:
                    return RedirectToAction("LocationChoiceSelection", "Apprenticeships");
                case ApprenticeshipDelivery.YourLocation:
                    return RedirectToAction("DeliveryOptions", "Apprenticeships");
                default:
                    return View("../ApprenticeShips/Delivery/Index");
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult LocationChoiceSelection(bool? national)
        {
            var model = new LocationChoiceSelectionViewModel
            {
                NationalApprenticeship = national.HasValue
                    ? national.Value
                        ? NationalApprenticeship.Yes
                        : NationalApprenticeship.No
                    : NationalApprenticeship.Undefined
            };

            return View("../Apprenticeships/LocationChoiceSelection/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult LocationChoiceSelection(LocationChoiceSelectionViewModel model)
        {
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            switch (model.NationalApprenticeship)
            {
                case NationalApprenticeship.Yes:
                    apprenticeship.ApprenticeshipLocations.RemoveAll(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased);

                    apprenticeship.ApprenticeshipLocations.Add(new ApprenticeshipLocation()
                    {
                        ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                        DeliveryModes = new List<int> { (int)ApprenticeshipDeliveryMode.EmployerAddress },
                        Id = Guid.NewGuid(),
                        National = true,
                    });

                    _session.SetObject("selectedApprenticeship", apprenticeship);
                    return RedirectToAction("Summary", "Apprenticeships", new {});

                case NationalApprenticeship.No:
                    return RedirectToAction("Regions", "Apprenticeships", new {});

                default:
                    return View("../ApprenticeShips/Search/Index");
            }
        }

        [Authorize]
        public IActionResult DeliveryOptions(string message)
        {
            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var model = new DeliveryOptionsViewModel();

            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");
            model.BlockRelease = false;
            model.DayRelease = false;
            model.Radius = null;
            model.locations = apprenticeship?.ApprenticeshipLocations.Where(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBased).ToList();
            model.HasOtherDeliveryOptions = apprenticeship?.ApprenticeshipLocations.Any(x =>
                x.ApprenticeshipLocationType != ApprenticeshipLocationType.ClassroomBased)?? false;
            ViewBag.Message = message;

            return View("../Apprenticeships/DeliveryOptions/Index", model);
        }

        [Authorize]
        public IActionResult DeliveryOptionsCombined(string message)
        {
            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var model = new DeliveryOptionsCombined();

            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            model.BlockRelease = false;
            model.DayRelease = false;
            model.National = false;
            model.Radius = null;
            model.Locations = apprenticeship?.ApprenticeshipLocations.Where(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBasedAndEmployerBased).ToList();
            model.HasOtherDeliveryOptions = apprenticeship?.ApprenticeshipLocations.Any(x =>
                                                x.ApprenticeshipLocationType != ApprenticeshipLocationType.ClassroomBasedAndEmployerBased) ?? false;

            ViewBag.Message = message;
            return View("../Apprenticeships/DeliveryOptionsCombined/Index", model);
        }

        [Authorize]
        public async Task<IActionResult> ViewAndEdit(ViewAndEditRequestModel requestModel)
        {
            int UKPRN;

            if (_session.GetInt32("UKPRN") != null)
            {
                UKPRN = _session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var model = new SummaryViewModel();

            if (requestModel.Mode != ApprenticeshipMode.Undefined)
            {
                _session.SetObject("ApprenticeshipMode", requestModel.Mode);
            }
            else
            {
                model.Mode = _session.GetObject<ApprenticeshipMode>("ApprenticeshipMode");
            }

            if (!Guid.TryParse(requestModel.Id, out var apprenticeshipId))
            {
                return BadRequest();
            }

            var result = await _sqlQueryDispatcher.ExecuteQuery(new GetApprenticeship { ApprenticeshipId = apprenticeshipId });

            if (result == null)
            {
                return NotFound();
            }

            var selectedApprenticeship = Apprenticeship.FromSqlModel(result);

            model.Apprenticeship = selectedApprenticeship;

            var type = result.ApprenticeshipLocations.FirstOrDefault();

            model.Regions = selectedApprenticeship.ApprenticeshipLocations.Any(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased)
                ? SubRegionCodesToDictionary(selectedApprenticeship.ApprenticeshipLocations.FirstOrDefault(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased)?.SubRegionIds)
                : null;

            model.SummaryOnly = true;
            _session.SetObject("selectedApprenticeship", selectedApprenticeship);

            return View("../Apprenticeships/Summary/Index", model);
        }

        [Authorize]
        public IActionResult Cancel()
        {
            _session.Remove("selectedApprenticeship");
            return RedirectToAction("Index", "ProviderApprenticeships", new { });
        }

        [Authorize]
        public IActionResult Summary(SummaryRequestModel requestModel)
        {
            int UKPRN;

            if (_session.GetInt32("UKPRN") != null)
            {
                UKPRN = _session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var mode = _session.GetObject<ApprenticeshipMode>("ApprenticeshipMode");
            var selectedApprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");


            var model = new SummaryViewModel();

            if (selectedApprenticeship != null)
            {
                model.Apprenticeship = selectedApprenticeship;
                model.Regions = model.Regions = selectedApprenticeship.ApprenticeshipLocations.Any(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased)
                    ? SubRegionCodesToDictionary(selectedApprenticeship.ApprenticeshipLocations.FirstOrDefault(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased)?.SubRegionIds) : null;

                model.HasAllLocationTypes = model.Apprenticeship.ApprenticeshipLocations.Any(x =>
                                                x.ApprenticeshipLocationType ==
                                                ApprenticeshipLocationType.ClassroomBased) &&
                                            model.Apprenticeship.ApprenticeshipLocations.Any(x =>
                                                x.ApprenticeshipLocationType == ApprenticeshipLocationType
                                                    .ClassroomBasedAndEmployerBased) &&
                                            model.Apprenticeship.ApprenticeshipLocations.Any(x =>
                                                x.ApprenticeshipLocationType ==
                                                ApprenticeshipLocationType.EmployerBased);
            }

            model.SummaryOnly = requestModel.SummaryOnly;
            model.Mode = mode;

            return View("../Apprenticeships/Summary/Index", model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Summary(SummaryViewModel theModel)
        {
            int UKPRN;

            if (_session.GetInt32("UKPRN") != null)
            {
                UKPRN = _session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            var locations = apprenticeship.ApprenticeshipLocations
                .Select(l => new CreateApprenticeshipLocation()
                {
                    ApprenticeshipLocationId = l.Id,
                    ApprenticeshipLocationType = l.ApprenticeshipLocationType,
                    DeliveryModes = l.DeliveryModes.Cast<ApprenticeshipDeliveryMode>(),
                    National = l.National,
                    Radius = l.Radius,
                    SubRegionIds = l.SubRegionIds,
                    VenueId = l.VenueId
                });

            var mode =_session.GetObject<ApprenticeshipMode>("ApprenticeshipMode");

            if (mode == ApprenticeshipMode.EditYourApprenticeships)
            {
                var result = await _sqlQueryDispatcher.ExecuteQuery(new UpdateApprenticeship()
                {
                    ApprenticeshipId = apprenticeship.id,
                    ApprenticeshipWebsite = apprenticeship.Url,
                    ContactEmail = apprenticeship.ContactEmail,
                    ContactTelephone = apprenticeship.ContactTelephone,
                    ContactWebsite = apprenticeship.ContactWebsite,
                    UpdatedBy = _currentUserProvider.GetCurrentUser(),
                    UpdatedOn = _clock.UtcNow,
                    MarketingInformation = apprenticeship.MarketingInformation,
                    Standard = await _standardsAndFrameworksCache.GetStandard(apprenticeship.StandardCode, apprenticeship.StandardVersion),
                    ApprenticeshipLocations = locations
                });

                return result.Match(
                    _ => RedirectToAction("Summary", "Apprenticeships"),
                    _ => RedirectToAction("Index", "ProviderApprenticeships", new { apprenticeshipId = apprenticeship.id, message = "You edited " + apprenticeship.ApprenticeshipTitle }));
            }
            else
            {
                await _sqlQueryDispatcher.ExecuteQuery(new CreateApprenticeship()
                {
                    ApprenticeshipId = Guid.NewGuid(),
                    ApprenticeshipWebsite = apprenticeship.Url,
                    ContactEmail = apprenticeship.ContactEmail,
                    ContactTelephone = apprenticeship.ContactTelephone,
                    ContactWebsite = apprenticeship.ContactWebsite,
                    CreatedBy = _currentUserProvider.GetCurrentUser(),
                    CreatedOn = _clock.UtcNow,
                    MarketingInformation = apprenticeship.MarketingInformation,
                    ProviderId = _providerContextProvider.GetProviderId(withLegacyFallback: true),
                    Standard = await _standardsAndFrameworksCache.GetStandard(apprenticeship.StandardCode, apprenticeship.StandardVersion),
                    ApprenticeshipLocations = locations
                });

                return RedirectToAction("Complete", "Apprenticeships");
            }
        }

        [Authorize]
        public IActionResult Regions()
        {
            var model = new RegionsViewModel();
            model.ChooseRegion = new ChooseRegionModel
            {
                Regions = _courseService.GetRegions(),
                UseNationalComponent = false
            };

            var Apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");
            var employerBased = Apprenticeship?.ApprenticeshipLocations.FirstOrDefault(x =>
                x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased);

            model.ChooseRegion.HasOtherDeliveryOptions = Apprenticeship?.ApprenticeshipLocations.Any(x =>
                                                      x.ApprenticeshipLocationType != ApprenticeshipLocationType.EmployerBased) ?? false;
            if (Apprenticeship != null && employerBased != null)
            {
                foreach (var selectRegionRegionItem in model.ChooseRegion.Regions.RegionItems.OrderBy(x => x.RegionName))
                {
                    foreach (var subRegionItemModel in selectRegionRegionItem.SubRegion)
                    {
                        if (employerBased.SubRegionIds.Contains(subRegionItemModel.Id))
                        {
                            subRegionItemModel.Checked = true;
                        }
                    }
                }
            }

            return View("../Apprenticeships/Regions/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Regions(string[] SelectedRegions, RegionsViewModel model)
        {
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            apprenticeship.ApprenticeshipLocations.RemoveAll(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased);

            if (SelectedRegions.Any())
            {
                apprenticeship.ApprenticeshipLocations.Add(new ApprenticeshipLocation()
                {
                    ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                    DeliveryModes = new List<int> { (int)ApprenticeshipDeliveryMode.EmployerAddress },
                    Id = Guid.NewGuid(),
                    National = false,
                    SubRegionIds = SelectedRegions
                });
            }

            _session.SetObject("selectedApprenticeship", apprenticeship);

            return RedirectToAction("Summary", "Apprenticeships");
        }

        [HttpPost]
        public async Task<IActionResult> AddCombined(AddDeliveryOptionsCombinedModel model)
        {
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            if (!model.LocationId.HasValue)
            {
                return RedirectToAction("Summary", "Apprenticeships");
            }

            var venue = await _sqlQueryDispatcher.ExecuteQuery(new GetVenue() { VenueId = model.LocationId.Value });

            var deliveryModes = new List<ApprenticeshipDeliveryMode>() { ApprenticeshipDeliveryMode.EmployerAddress };
            if (model.BlockRelease)
            {
                deliveryModes.Add(ApprenticeshipDeliveryMode.BlockRelease);
            }
            if (model.DayRelease)
            {
                deliveryModes.Add(ApprenticeshipDeliveryMode.DayRelease);
            }

            apprenticeship.ApprenticeshipLocations.Add(new ApprenticeshipLocation()
            {
                Id = Guid.NewGuid(),
                ApprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBasedAndEmployerBased,
                DeliveryModes = deliveryModes.Cast<int>().ToList(),
                National = model.National,
                Radius = int.Parse(model.Radius),
                VenueId = venue.VenueId
            });

            _session.SetObject("selectedApprenticeship", apprenticeship);

            return RedirectToAction("DeliveryOptionsCombined", "Apprenticeships");
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddDeliveryOptionViewModel model)
        {
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            if (!model.LocationId.HasValue)
            {
                return RedirectToAction("Summary", "Apprenticeships");
            }

            var venue = await _sqlQueryDispatcher.ExecuteQuery(new GetVenue() { VenueId = model.LocationId.Value });

            var deliveryModes = new List<ApprenticeshipDeliveryMode>();
            if (model.BlockRelease)
            {
                deliveryModes.Add(ApprenticeshipDeliveryMode.BlockRelease);
            }
            if (model.DayRelease)
            {
                deliveryModes.Add(ApprenticeshipDeliveryMode.DayRelease);
            }

            apprenticeship.ApprenticeshipLocations.Add(new ApprenticeshipLocation()
            {
                ApprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBased,
                DeliveryModes = deliveryModes.Cast<int>().ToList(),
                Id = Guid.NewGuid(),
                National = false,
                Name = venue.VenueName,
                Radius = _apprenticeshipSettings.Value.DefaultRadius,
                Venue = venue,
                VenueId = venue.VenueId
            });

            _session.SetObject("selectedApprenticeship", apprenticeship);

            return RedirectToAction("DeliveryOptions", "Apprenticeships");
        }

        public async Task<IActionResult> Continue(string LocationId, bool DayRelease, bool BlockRelease, int RowCount)
        {
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            var venue = await _sqlQueryDispatcher.ExecuteQuery(new GetVenue() { VenueId = Guid.Parse(LocationId) });

            var deliveryModes = new List<ApprenticeshipDeliveryMode>();
            if (BlockRelease)
            {
                deliveryModes.Add(ApprenticeshipDeliveryMode.BlockRelease);
            }
            if (DayRelease)
            {
                deliveryModes.Add(ApprenticeshipDeliveryMode.DayRelease);
            }

            apprenticeship.ApprenticeshipLocations.Add(new ApprenticeshipLocation()
            {
                ApprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBased,
                DeliveryModes = deliveryModes.Cast<int>().ToList(),
                Id = Guid.NewGuid(),
                National = false,
                Name = venue.VenueName,
                Radius = _apprenticeshipSettings.Value.DefaultRadius,
                Venue = venue,
                VenueId = venue.VenueId
            });

            _session.SetObject("selectedApprenticeship", apprenticeship);

            return Json(Url.Action("Summary", "Apprenticeships", new {}));
        }

        public async Task<IActionResult> ContinueCombined(string LocationId, bool DayRelease, bool BlockRelease, bool National, string Radius, int RowCount)
        {
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            var venue = await _sqlQueryDispatcher.ExecuteQuery(new GetVenue() { VenueId = Guid.Parse(LocationId) });

            var deliveryModes = new List<ApprenticeshipDeliveryMode>() { ApprenticeshipDeliveryMode.EmployerAddress };
            if (BlockRelease)
            {
                deliveryModes.Add(ApprenticeshipDeliveryMode.BlockRelease);
            }
            if (DayRelease)
            {
                deliveryModes.Add(ApprenticeshipDeliveryMode.DayRelease);
            }

            var radiusValue = National
                ? _apprenticeshipSettings.Value.NationalRadius
                : Convert.ToInt32(Radius);

            apprenticeship.ApprenticeshipLocations.Add(new ApprenticeshipLocation()
            {
                Id = Guid.NewGuid(),
                ApprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBasedAndEmployerBased,
                DeliveryModes = deliveryModes.Cast<int>().ToList(),
                National = National,
                Radius = radiusValue,
                VenueId = venue.VenueId
            });

            _session.SetObject("selectedApprenticeship", apprenticeship);

            return Json(Url.Action("Summary", "Apprenticeships", new {}));
        }

        [Authorize]
        public IActionResult Complete(CompleteViewModel model)
        {

            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            model.ApprenticeshipName = apprenticeship?.ApprenticeshipTitle;

            _session.Remove("selectedApprenticeship");
            _session.Remove("ApprenticeshipMode");
            return View("../Apprenticeships/Complete/Index", model);
        }

        [Authorize]
        public IActionResult AddAnotherApprenticeship()
        {
            return RedirectToAction("Index", "Apprenticeships");
        }


        [Authorize]
        public IActionResult WhatWouldYouLIkeToDo()
        {
            var model = new WhatWouldYouLikeToDoViewModel();


            return View("../Apprenticeships/WhatWouldYouLikeToDo/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult WhatWouldYouLIkeToDo(WhatWouldYouLikeToDoViewModel theModel)
        {
            switch (theModel.ApprenticeshipWhatWouldYouLikeToDo)
            {
                case ApprenticeshipWhatWouldYouLikeToDo.Add:
                    return RedirectToAction("Index", "Apprenticeships");
                case ApprenticeshipWhatWouldYouLikeToDo.Upload:
                    if (_featureFlagProvider.HaveFeature(FeatureFlags.ApprenticeshipsDataManagement))
                    {
                        return RedirectToAction("Index", "ApprenticeshipsDataManagement")
                            .WithProviderContext(_providerContextProvider.GetProviderContext(withLegacyFallback: true));
                    }
                    else
                    {
                        return RedirectToAction("Index", "BulkUploadApprenticeships");
                    }
                case ApprenticeshipWhatWouldYouLikeToDo.View:
                    return RedirectToAction("Index", "ProviderApprenticeships");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        public IActionResult DeleteConfirm(Guid apprenticeshipId, string apprenticeshipTitle)
        {
            var model = new DeleteConfirmViewModel();
            model.ApprenticeshipId = apprenticeshipId;
            model.ApprenticeshipTitle = apprenticeshipTitle;

            return View("../Apprenticeships/ConfirmationDelete/Index", model);
        }

        [Authorize]
        public IActionResult ConfirmationDelete(Guid ApprenticeshipId, string ApprenticeshipTitle, int level)
        {
            var model = new ConfirmationDeleteViewModel();
            model.ApprenticeshipId = ApprenticeshipId;
            model.ApprenticeshipTitle = ApprenticeshipTitle;
            model.Level = level;

            return View("../Apprenticeships/ConfirmApprenticeshipDelete/Index", model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ConfirmationDelete(ConfirmationDeleteViewModel theModel)
        {
            _session.Remove("selectedApprenticeship");

            switch (theModel.ApprenticeshipDelete)
            {
                case ApprenticeshipDelete.Delete:
                    var deleteResult = await _sqlQueryDispatcher.ExecuteQuery(new DeleteApprenticeship()
                    {
                        ApprenticeshipId = theModel.ApprenticeshipId,
                        DeletedBy = _currentUserProvider.GetCurrentUser(),
                        DeletedOn = _clock.UtcNow
                    });
                    if (deleteResult.Value is Success)
                    {
                        return RedirectToAction("DeleteConfirm", "Apprenticeships", new { ApprenticeshipId = theModel.ApprenticeshipId, ApprenticeshipTitle = theModel.ApprenticeshipTitle });
                    }
                    return RedirectToAction("Index", "ProviderApprenticeships");

                case ApprenticeshipDelete.Back:
                    return RedirectToAction("Index", "ProviderApprenticeships");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        public IActionResult DeleteDeliveryOption(string LocationName, ApprenticeshipMode Mode, Guid Id)
        {
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");
            var Location = apprenticeship?.ApprenticeshipLocations.FirstOrDefault(x => x.Id == Id);
            if (apprenticeship == null || Location == null)
            {
                return RedirectToAction("Index", "ProviderApprenticeships", new { });
            }

            var model = new DeleteDeliveryOptionViewModel();

            model.Combined = Location.ApprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBasedAndEmployerBased;
            model.LocationName = LocationName;

            model.ApprenticeshipTitle = Location?.Name;

            return View("../Apprenticeships/DeleteDeliveryOption/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult DeleteDeliveryOption(DeleteDeliveryOptionViewModel model)
        {
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");
            var Location = apprenticeship?.ApprenticeshipLocations.FirstOrDefault(x => x.Id == model.Id);
            if (apprenticeship == null || Location == null)
            {
                return RedirectToAction("Index", "ProviderApprenticeships", new { });
            }
            apprenticeship.ApprenticeshipLocations.RemoveAll(x => x.Id == model.Id);

            _session.SetObject("selectedApprenticeship", apprenticeship);
            return RedirectToAction(model.Combined ? "DeliveryOptionsCombined" : "DeliveryOptions", "Apprenticeships",
                new { message = "Venue " + model.LocationName + " deleted"});
        }

        [Authorize]
        public IActionResult AddNewVenue(ApprenticeshipLocationType type)
        {
            var returnUrl = type == ApprenticeshipLocationType.ClassroomBasedAndEmployerBased ?
                Url.Action("DeliveryOptionsCombined", "Apprenticeships", new
                {
                    message = string.Empty,
                    mode = "Add"
                }) :
                Url.Action("DeliveryOptions", "Apprenticeships", new
                {
                    message = string.Empty,
                    mode = "Add"
                });

            return Json(new Url(Url.Action("Index", "AddVenue", new { returnUrl }))
                .WithProviderContext(_providerContextProvider.GetProviderContext(withLegacyFallback: true))
                .ToString());
        }

        private Dictionary<string, List<string>> SubRegionCodesToDictionary(IEnumerable<string> subRegions)
        {
            SelectRegionModel selectRegionModel = new SelectRegionModel();
            Dictionary<string, List<string>> regionsAndSubregions = new Dictionary<string, List<string>>();

            foreach (var subRegionCode in subRegions ?? Array.Empty<string>())
            {
                var isRegionCode = selectRegionModel.RegionItems.FirstOrDefault(x => String.Equals(x.Id, subRegionCode, StringComparison.CurrentCultureIgnoreCase));

                if (isRegionCode != null)
                {
                    if (!regionsAndSubregions.ContainsKey(isRegionCode.RegionName))
                    {
                        var subRegionNamesList = isRegionCode.SubRegion.Select(x => x.SubRegionName).ToList();
                        regionsAndSubregions.Add(isRegionCode.RegionName, subRegionNamesList);

                    }
                }
                else
                {
                    var regionName = selectRegionModel.GetRegionNameForSubRegion(subRegionCode);
                    if (string.IsNullOrWhiteSpace(regionName)) continue;

                    if (!regionsAndSubregions.ContainsKey(regionName))
                    {
                        regionsAndSubregions.Add(regionName, new List<string>());
                    }
                    var subRegionItem = selectRegionModel.GetSubRegionItemByRegionCode(subRegionCode);
                    var alreadyExists = CheckForExistingSubregions(subRegionItem.SubRegionName,
                        regionsAndSubregions[regionName]);
                    if(alreadyExists == false)
                        regionsAndSubregions[regionName].Add(subRegionItem.SubRegionName);
                }
            }

            return regionsAndSubregions;
        }

        private bool CheckForExistingSubregions(string subregionName, List<string> subregions)
        {
            return subregions.Contains(subregionName);
        }

        private async Task<Apprenticeship> MapToApprenticeship(DetailViewModel model, int ukprn, List<ApprenticeshipLocation> locations)
        {
            var providerId = (await _providerInfoCache.GetProviderIdForUkprn(ukprn)).Value;

            return new Apprenticeship
            {
                ProviderId = providerId,
                ProviderUkprn = ukprn,
                ApprenticeshipTitle = model.ApprenticeshipTitle,
                ApprenticeshipType = model.ApprenticeshipType,
                StandardCode = model.StandardCode,
                MarketingInformation = model.Information,
                Url = model.Website,
                ContactTelephone = model.Telephone,
                ContactEmail = model.Email,
                ContactWebsite = model.ContactUsIUrl,
                CreatedDate = DateTime.Now,
                StandardVersion = model.Version,
                NotionalNVQLevelv2 = model.NotionalNVQLevelv2,
                ApprenticeshipLocations = locations,
                UpdatedDate = DateTime.UtcNow,
            };
        }

        private DetailViewModel MapToDetailViewModel(Apprenticeship apprenticeship, ApprenticeshipMode mode)
        {
            return new DetailViewModel
            {
                ApprenticeshipTitle = apprenticeship.ApprenticeshipTitle,
                ApprenticeshipType = apprenticeship.ApprenticeshipType,
                StandardCode = apprenticeship.StandardCode,
                Information = apprenticeship.MarketingInformation,
                Website = apprenticeship.Url,
                Telephone = apprenticeship.ContactTelephone,
                Email = apprenticeship.ContactEmail,
                ContactUsIUrl = apprenticeship.ContactWebsite,
                NotionalNVQLevelv2 = apprenticeship.NotionalNVQLevelv2,
                Mode = mode
            };
        }
    }
}
