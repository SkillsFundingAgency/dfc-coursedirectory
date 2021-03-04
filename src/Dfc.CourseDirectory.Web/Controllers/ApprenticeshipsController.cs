using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.ApprenticeshipService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Apprenticeships;
using Dfc.CourseDirectory.Services.Models.Regions;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Configuration;
using Dfc.CourseDirectory.Web.Extensions;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.Helpers.Attributes;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships;
using Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships.ApprenticeshipSearchResult;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseRegion;
using Dfc.CourseDirectory.Web.ViewModels.Apprenticeships;
using Dfc.CourseDirectory.WebV2;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Controllers
{
    [Authorize("Apprenticeship")]
    [SelectedProviderNeeded]
    [RestrictApprenticeshipQAStatus(Core.Models.ApprenticeshipQAStatus.Passed)]
    public class ApprenticeshipsController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;
        private readonly IApprenticeshipService _apprenticeshipService;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IOptions<ApprenticeshipSettings> _apprenticeshipSettings;
        private readonly IStandardsAndFrameworksCache _standardsAndFrameworksCache;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;

        private ISession _session => HttpContext.Session;

        public ApprenticeshipsController(
            ICourseService courseService,
            IVenueService venueService,
            IApprenticeshipService apprenticeshipService,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            IOptions<ApprenticeshipSettings> apprenticeshipSettings,
            IStandardsAndFrameworksCache standardsAndFrameworksCache,
            ICurrentUserProvider currentUserProvider,
            IClock clock)
        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _venueService = venueService ?? throw new ArgumentNullException(nameof(venueService));
            _apprenticeshipService = apprenticeshipService ?? throw new ArgumentNullException(nameof(apprenticeshipService));
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
            _apprenticeshipSettings = apprenticeshipSettings ?? throw new ArgumentNullException(nameof(apprenticeshipSettings));
            _standardsAndFrameworksCache = standardsAndFrameworksCache ?? throw new ArgumentNullException(nameof(standardsAndFrameworksCache));
            _currentUserProvider = currentUserProvider ?? throw new ArgumentNullException(nameof(currentUserProvider));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
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
                    id = r.CosmosId,
                    StandardCode = r.StandardCode,
                    Version = r.Version,
                    OtherBodyApprovalRequired = r.OtherBodyApprovalRequired,
                    ApprenticeshipType = ApprenticeshipType.StandardCode,
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
                model.Id = request.Id;
                model.ApprenticeshipTitle = request.ApprenticeshipTitle;
                model.ApprenticeshipPreviousPage = request.PreviousPage;
                model.ApprenticeshipType = request.ApprenticeshipType;
                model.ProgType = request.ProgType;
                model.PathwayCode = request.PathwayCode;
                model.Version = request.Version.HasValue ? request.Version.Value : (int?)null;
                model.NotionalNVQLevelv2 = request.NotionalNVQLevelv2;
                model.Cancelled = request.Cancelled.HasValue && request.Cancelled.Value;
                model.Mode = mode;
                switch (request.ApprenticeshipType)
                {
                    case ApprenticeshipType.StandardCode:
                        {
                            model.StandardCode = request.StandardCode;
                            break;
                        }
                    case ApprenticeshipType.FrameworkCode:
                        {
                            model.FrameworkCode = request.FrameworkCode;
                            break;
                        }
                }
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
                apprenticeship.FrameworkCode = model.FrameworkCode;
                apprenticeship.ProgType = model.ProgType;
                apprenticeship.MarketingInformation = model.Information;
                apprenticeship.Url = model.Website;
                apprenticeship.ContactTelephone = model.Telephone;
                apprenticeship.ContactEmail = model.Email;
                apprenticeship.ContactWebsite = model.ContactUsIUrl;
                apprenticeship.PathwayCode = model.PathwayCode;
                apprenticeship.NotionalNVQLevelv2 = model.NotionalNVQLevelv2;
            }

            _session.SetObject("selectedApprenticeship", apprenticeship);

            switch (model.Mode)
            {
                case ApprenticeshipMode.Add:
                    if (model.ShowCancelled.HasValue && model.ShowCancelled.Value == true)
                    {
                        return RedirectToAction("Summary", "Apprenticeships", new SummaryRequestModel() { Id = model.Id.ToString(), SummaryOnly = false });
                    }
                    return RedirectToAction("Delivery", "Apprenticeships", new DeliveryRequestModel() { Mode = model.Mode });
                case ApprenticeshipMode.EditApprenticeship:
                    return RedirectToAction("Summary", "Apprenticeships",
                        new SummaryRequestModel() { SummaryOnly = false });
                case ApprenticeshipMode.EditYourApprenticeships:
                    return RedirectToAction("Summary", "Apprenticeships", new SummaryRequestModel() { Id = model.Id.ToString(), SummaryOnly = false });
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

            var location = apprenticeship.ApprenticeshipLocations.FirstOrDefault(x =>
                x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased);

            switch (model.NationalApprenticeship)
            {
                case NationalApprenticeship.Yes:

                    if (location == null)
                    {
                        apprenticeship.ApprenticeshipLocations.Add(CreateRegionLocation(new string[0], true));
                    }
                    else
                    {
                        apprenticeship.ApprenticeshipLocations.RemoveAll(x =>
                            x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased);
                        apprenticeship.ApprenticeshipLocations.Add(CreateRegionLocation(new string[0], true));
                    }

                    _session.SetObject("selectedApprenticeship", apprenticeship);
                    return RedirectToAction("Summary", "Apprenticeships", new {});

                case NationalApprenticeship.No:

                    if (location != null)
                    {
                        apprenticeship.ApprenticeshipLocations.RemoveAll(x =>
                            x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased);
                        apprenticeship.ApprenticeshipLocations.Add(CreateRegionLocation(new string[0], false));
                    }
                    _session.SetObject("selectedApprenticeship", apprenticeship);
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

            var result = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetApprenticeshipById { ApprenticeshipId = apprenticeshipId });

            if (result == null || result.RecordStatus != (int)RecordStatus.Live)
            {
                return NotFound();
            }

            var selectedApprenticeship = Apprenticeship.FromCosmosDbModel(result);

            model.Apprenticeship = selectedApprenticeship;

            var type = result.ApprenticeshipLocations.FirstOrDefault();

            model.Regions = selectedApprenticeship.ApprenticeshipLocations.Any(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased)
                ? SubRegionCodesToDictionary(selectedApprenticeship.ApprenticeshipLocations.FirstOrDefault(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased)?.Regions)
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
                    ? SubRegionCodesToDictionary(selectedApprenticeship.ApprenticeshipLocations.FirstOrDefault(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased)?.Regions) : null;

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

            var mode =_session.GetObject<ApprenticeshipMode>("ApprenticeshipMode");

            if (mode == ApprenticeshipMode.EditYourApprenticeships)
            {
                var result = await _apprenticeshipService.UpdateApprenticeshipAsync(apprenticeship);

                if (result.IsSuccess)
                {
                    return RedirectToAction("Index", "ProviderApprenticeships", new { apprenticeshipId = result.Value.id, message = "You edited " + result.Value.ApprenticeshipTitle });
                }
                else
                {
                    //Action needs to be decided if failure
                    return RedirectToAction("Summary", "Apprenticeships");
                }
            }
            else
            {
                var result = await _cosmosDbQueryDispatcher.ExecuteQuery(new CreateApprenticeship
                {
                    Id = Guid.NewGuid(),
                    ProviderId = apprenticeship.ProviderId,
                    ProviderUkprn = apprenticeship.ProviderUKPRN,
                    ApprenticeshipTitle = apprenticeship.ApprenticeshipTitle,
                    ApprenticeshipType = apprenticeship.ApprenticeshipType,
                    StandardOrFramework = await _standardsAndFrameworksCache.GetStandard(apprenticeship.StandardCode.Value, apprenticeship.Version.Value),
                    MarketingInformation = apprenticeship.MarketingInformation,
                    Url = apprenticeship.Url,
                    ContactTelephone = apprenticeship.ContactTelephone,
                    ApprenticeshipLocations = apprenticeship.ApprenticeshipLocations.Where(al => al != null).Select(al => new CreateApprenticeshipLocation
                    {
                        Id = al.Id,
                        VenueId = al.VenueId,
                        National = al.National,
                        Address = al.Address != null
                            ? new Core.DataStore.CosmosDb.Models.ApprenticeshipLocationAddress
                            {
                                Address1 = al.Address.Address1,
                                Address2 = al.Address.Address2,
                                County = al.Address.County,
                                Email = al.Address.Email,
                                Latitude = al.Address.Latitude ?? 0,
                                Longitude = al.Address.Longitude ?? 0,
                                Phone = al.Address.Phone,
                                Postcode = al.Address.Postcode,
                                Town = al.Address.Town,
                                Website = al.Address.Website
                            }
                            : null,
                        DeliveryModes = al.DeliveryModes.Cast<ApprenticeshipDeliveryMode>().ToList(),
                        Name = al.Name,
                        Phone = al.Phone,
                        Regions = al.Regions,
                        ApprenticeshipLocationType = al.ApprenticeshipLocationType,
                        LocationType = al.LocationType,
                        Radius = al.Radius
                    }),
                    CreatedDate = _clock.UtcNow,
                    CreatedByUser = _currentUserProvider.GetCurrentUser(),
                    Status = (int)apprenticeship.RecordStatus
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
                        if (employerBased.Regions.Contains(subRegionItemModel.Id))
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
            var Apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            var employerBased = Apprenticeship.ApprenticeshipLocations.FirstOrDefault(x =>
                 x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased);

            if (SelectedRegions.Any())
            {
                if (employerBased != null)
                {
                    employerBased.Regions = SelectedRegions;
                }
                else
                {
                    Apprenticeship.ApprenticeshipLocations.Add(CreateRegionLocation(SelectedRegions, null));
                }
            }
            else
            {
                Apprenticeship.ApprenticeshipLocations.RemoveAll(x=>x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased);
            }

            _session.SetObject("selectedApprenticeship", Apprenticeship);

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

            var venue = await _venueService.GetVenueByIdAsync(
                new GetVenueByIdCriteria(model.LocationId.Value.ToString()));

            string deliveryMethod;
            if (model.BlockRelease && model.DayRelease)
            {
                deliveryMethod = "Employer address, Day release, Block release";
            }
            else
            {
                deliveryMethod = model.DayRelease
                    ? "Employer address, Day release"
                    : "Employer address, Block release";
            }

            apprenticeship.ApprenticeshipLocations.Add(CreateDeliveryLocation(
                new DeliveryOption()
                {
                    Delivery = deliveryMethod,
                    LocationId = venue.Value.ID.ToString(),
                    LocationName = venue.Value.VenueName,
                    PostCode = venue.Value.PostCode,
                    Radius = model.Radius,
                    National = model.National,
                    Venue = venue.Value
                },
                ApprenticeshipLocationType.ClassroomBasedAndEmployerBased));

            _session.SetObject("selectedApprenticeship", apprenticeship);

            return RedirectToAction("DeliveryOptionsCombined", "Apprenticeships");
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddDeliveryOptionViewModel model)
        {
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            if (apprenticeship.ApprenticeshipLocations == null)
            {
                apprenticeship.ApprenticeshipLocations = new List<ApprenticeshipLocation>();
            }

            if (!model.LocationId.HasValue)
            {
                return RedirectToAction("Summary", "Apprenticeships");
            }

            var venue = await _venueService.GetVenueByIdAsync(
                new GetVenueByIdCriteria(model.LocationId.Value.ToString()));

            string deliveryMethod;
            if (model.BlockRelease && model.DayRelease)
            {
                deliveryMethod = "Day release, Block release";
            }
            else
            {
                deliveryMethod = model.DayRelease ? "Day release" : "Block release";
            }

            apprenticeship.ApprenticeshipLocations.Add(CreateDeliveryLocation(
                new DeliveryOption()
                {
                    Delivery = deliveryMethod,
                    LocationId = venue.Value.ID.ToString(),
                    LocationName = venue.Value.VenueName,
                    PostCode = venue.Value.PostCode,
                    Venue = venue.Value,
                    Radius = _apprenticeshipSettings.Value.DefaultRadius.ToString()
                },
                ApprenticeshipLocationType.ClassroomBased));

            _session.SetObject("selectedApprenticeship", apprenticeship);

            return RedirectToAction("DeliveryOptions", "Apprenticeships");
        }

        public async Task<IActionResult> Continue(string LocationId, bool DayRelease, bool BlockRelease, int RowCount)
        {
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            if (apprenticeship.ApprenticeshipLocations == null)
            {
                apprenticeship.ApprenticeshipLocations = new List<ApprenticeshipLocation>();
            }

            var venue = await _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(LocationId));

            string deliveryMethod;
            if (BlockRelease && DayRelease)
            {
                deliveryMethod = "Day release, Block release";
            }
            else
            {
                deliveryMethod = DayRelease ? "Day release" : "Block release";
            }

            apprenticeship.ApprenticeshipLocations.Add(CreateDeliveryLocation(
                new DeliveryOption()
                {
                    Delivery = deliveryMethod,
                    LocationId = venue.Value.ID.ToString(),
                    LocationName = venue.Value.VenueName,
                    PostCode = venue.Value.PostCode,
                    Venue = venue.Value,
                    Radius = _apprenticeshipSettings.Value.DefaultRadius.ToString()

                },
                ApprenticeshipLocationType.ClassroomBased));

            _session.SetObject("selectedApprenticeship", apprenticeship);

            return Json(Url.Action("Summary", "Apprenticeships", new {}));
        }

        public async Task<IActionResult> ContinueCombined(string LocationId, bool DayRelease, bool BlockRelease, bool National, string Radius, int RowCount)
        {
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            var venue = await _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(LocationId));

            string deliveryMethod;
            if (BlockRelease && DayRelease)
            {
                deliveryMethod = "Employers address, Day release, Block release";
            }
            else
            {
                deliveryMethod = DayRelease ? "Employers address, Day release" : "Employers address, Block release";
            }

            var radiusValue = National
                ? _apprenticeshipSettings.Value.NationalRadius
                : Convert.ToInt32(Radius);

            apprenticeship.ApprenticeshipLocations.Add(CreateDeliveryLocation(
                new DeliveryOption()
                {
                    Delivery = deliveryMethod,
                    LocationId = venue.Value.ID.ToString(),
                    LocationName = venue.Value.VenueName,
                    PostCode = venue.Value.PostCode,
                    National = National,
                    Radius = radiusValue.ToString(),
                    Venue = venue.Value
                },
                ApprenticeshipLocationType.ClassroomBasedAndEmployerBased));

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
                    return RedirectToAction("Index", "BulkUploadApprenticeships");
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
                    //call delete service
                    var result = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetApprenticeshipById { ApprenticeshipId = theModel.ApprenticeshipId });

                    if (result != null)
                    {
                        var getApprenticehipByIdResult = Apprenticeship.FromCosmosDbModel(result);

                        getApprenticehipByIdResult.RecordStatus = RecordStatus.Deleted;

                        var updateApprenticeshipResult = await _apprenticeshipService.UpdateApprenticeshipAsync(getApprenticehipByIdResult);

                        if (updateApprenticeshipResult.IsSuccess)
                        {

                            return RedirectToAction("DeleteConfirm", "Apprenticeships", new { ApprenticeshipId = theModel.ApprenticeshipId, ApprenticeshipTitle = theModel.ApprenticeshipTitle });
                        }

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
                new { message = "Location " + model.LocationName + " deleted"});
        }

        [Authorize]
        public IActionResult AddNewVenue(ApprenticeshipLocationType type)
        {
            _session.SetString("Option", type == ApprenticeshipLocationType.ClassroomBasedAndEmployerBased ?
                "AddNewVenueForApprenticeshipsCombined" :"AddNewVenueForApprenticeships");

            return Json(Url.Action("AddVenue", "Venues"));
        }

        private Dictionary<string, List<string>> SubRegionCodesToDictionary(IEnumerable<string> subRegions)
        {
            SelectRegionModel selectRegionModel = new SelectRegionModel();
            Dictionary<string, List<string>> regionsAndSubregions = new Dictionary<string, List<string>>();

            foreach (var subRegionCode in subRegions)
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

        private ApprenticeshipLocation CreateDeliveryLocation(DeliveryOption loc, ApprenticeshipLocationType apprenticeshipLocationType)
        {
            List<int> deliveryModes = new List<int>();

            ApprenticeshipLocation apprenticeshipLocation = new ApprenticeshipLocation()
            {
                Name = loc.LocationName,
                CreatedDate = DateTime.Now,
                CreatedBy =
                    User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault(),
                ApprenticeshipLocationType = apprenticeshipLocationType,
                Id = Guid.NewGuid(),
                LocationType = LocationType.Venue,
                RecordStatus = RecordStatus.Live,
                Regions = loc.Regions,
                National = loc.National ?? false,
                UpdatedDate = DateTime.Now,
                UpdatedBy =
                    User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault(),

            };
            if (loc.Venue != null)
            {
                apprenticeshipLocation.LocationId = loc.Venue.LocationId ?? null;
                apprenticeshipLocation.VenueId = Guid.Parse(loc.Venue.ID);
                apprenticeshipLocation.Address = new Address
                {
                    Address1 = loc.Venue.Address1,
                    Address2 = loc.Venue.Address2,
                    Town = loc.Venue.Town,
                    County = loc.Venue.County,
                    Postcode = loc.Venue.PostCode,
                    Email = loc.Venue.Email,
                    Phone = loc.Venue.Telephone,
                    Website = loc.Venue.Website,
                    Latitude = loc.Venue.Latitude,
                    Longitude = loc.Venue.Longitude
                };
            }
            if (!string.IsNullOrEmpty(loc.LocationId))
            {
                apprenticeshipLocation.LocationGuidId = new Guid(loc.LocationId);
            }

            if (!string.IsNullOrEmpty(loc.Radius))
            {
                apprenticeshipLocation.Radius = Convert.ToInt32(loc.Radius);
            }

            if (!string.IsNullOrEmpty(loc.Delivery))
            {
                var delModes = loc.Delivery.Split(",");

                foreach (var delMode in delModes)
                {
                    if (delMode.ToLower().Trim() ==
                        @WebHelper.GetEnumDescription(ApprenticeShipDeliveryLocation.DayRelease).ToLower())
                    {
                        deliveryModes.Add((int)ApprenticeShipDeliveryLocation.DayRelease);
                    }

                    if (delMode.ToLower().Trim() == @WebHelper
                            .GetEnumDescription(ApprenticeShipDeliveryLocation.BlockRelease).ToLower())
                    {
                        deliveryModes.Add((int)ApprenticeShipDeliveryLocation.BlockRelease);
                    }
                }
            }

            if (apprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBasedAndEmployerBased || apprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased)
            {
                deliveryModes.Add((int)ApprenticeShipDeliveryLocation.EmployerAddress);
            }

            apprenticeshipLocation.DeliveryModes = deliveryModes;

            return apprenticeshipLocation;
        }

        private async Task<Apprenticeship> MapToApprenticeship(DetailViewModel model, int ukprn, List<ApprenticeshipLocation> locations)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderByUkprn { Ukprn = ukprn });

            return new Apprenticeship
            {
                ProviderId = provider.Id,
                ProviderUKPRN = ukprn,
                ApprenticeshipTitle = model.ApprenticeshipTitle,
                ApprenticeshipType = model.ApprenticeshipType,
                StandardCode = model.StandardCode,
                FrameworkCode = model.FrameworkCode,
                ProgType = model.ProgType,
                MarketingInformation = model.Information,
                Url = model.Website,
                ContactTelephone = model.Telephone,
                ContactEmail = model.Email,
                ContactWebsite = model.ContactUsIUrl,
                CreatedDate = DateTime.Now,
                CreatedBy = User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault(),
                RecordStatus = RecordStatus.Live,
                PathwayCode = model.PathwayCode,
                Version = model.Version ?? (int?)null,
                NotionalNVQLevelv2 = model.NotionalNVQLevelv2,
                ApprenticeshipLocations = locations,
                UpdatedDate = DateTime.UtcNow,
                UpdatedBy = User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault()
            };
        }

        private DetailViewModel MapToDetailViewModel(Apprenticeship apprenticeship, ApprenticeshipMode mode)
        {
            return new DetailViewModel
            {
                ApprenticeshipTitle = apprenticeship.ApprenticeshipTitle,
                ApprenticeshipType = apprenticeship.ApprenticeshipType,
                StandardCode = apprenticeship.StandardCode,
                FrameworkCode = apprenticeship.FrameworkCode,
                ProgType = apprenticeship.ProgType,
                Information = apprenticeship.MarketingInformation,
                Website = apprenticeship.Url,
                Telephone = apprenticeship.ContactTelephone,
                Email = apprenticeship.ContactEmail,
                ContactUsIUrl = apprenticeship.ContactWebsite,
                PathwayCode = apprenticeship.PathwayCode,
                NotionalNVQLevelv2 = apprenticeship.NotionalNVQLevelv2,
                Mode = mode
            };
        }

        private ApprenticeshipLocation CreateRegionLocation(string[] regions, bool? national)
        {
            return CreateDeliveryLocation(new DeliveryOption
            {
                Regions = regions,
                National = national
            }, ApprenticeshipLocationType.EmployerBased);
        }
    }
}
