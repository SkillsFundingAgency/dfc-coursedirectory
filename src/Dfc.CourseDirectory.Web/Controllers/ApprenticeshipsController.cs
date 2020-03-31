using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using Dfc.CourseDirectory.Models.Models.Regions;
using Dfc.CourseDirectory.Models.Models.Venues;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.ProviderService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Extensions;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships;
using Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships.ApprenticeshipSearchResult;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseRegion;
using Dfc.CourseDirectory.Web.ViewModels.Apprenticeships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.Helpers.Attributes;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.Web.Controllers
{
    [Authorize("Apprenticeship")]
    [SelectedProviderNeeded]
    [RestrictApprenticeshipQAStatus(ApprenticeshipQAStatus.Passed, AllowWhenApprenticeshipQAFeatureDisabled = true)]
    public class ApprenticeshipsController : Controller
    {
        private readonly ILogger<ApprenticeshipsController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;
        private readonly IApprenticeshipService _apprenticeshipService;
        private readonly IProviderService _providerService;
        private readonly IOptions<ApprenticeshipSettings> _apprenticeshipSettings;


        public ApprenticeshipsController(
            ILogger<ApprenticeshipsController> logger,
            IHttpContextAccessor contextAccessor, ICourseService courseService, IVenueService venueService
            , IApprenticeshipService apprenticeshipService,
            IProviderService providerService, IOptions<ApprenticeshipSettings> apprenticeshipSettings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(venueService, nameof(venueService));
            Throw.IfNull(apprenticeshipService, nameof(apprenticeshipService));
            Throw.IfNull(providerService, nameof(providerService));
            Throw.IfNull(apprenticeshipSettings, nameof(apprenticeshipSettings));

            _apprenticeshipSettings = apprenticeshipSettings;
            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _venueService = venueService;
            _apprenticeshipService = apprenticeshipService;
            _providerService = providerService;
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

            ApprenticeshipsSearchResultModel model = new ApprenticeshipsSearchResultModel();

            if (!string.IsNullOrEmpty(requestModel.SearchTerm))
            {
                var result = await _apprenticeshipService.StandardsAndFrameworksSearch(requestModel.SearchTerm, UKPRN.Value);



                var listOfApprenticeships = new List<ApprenticeShipsSearchResultItemModel>();

                if (result.IsSuccess && result.HasValue)
                {
                    foreach (var item in result.Value)
                    {
                        listOfApprenticeships.Add(new ApprenticeShipsSearchResultItemModel()
                        {
                            ApprenticeshipTitle = item.StandardName ?? item.NasTitle,
                            id = item.id,
                            StandardCode = item.StandardCode,
                            Version = item.Version,
                            StandardSectorCode = item.StandardSectorCode,
                            URLLink = item.URLLink,
                            OtherBodyApprovalRequired = item.OtherBodyApprovalRequired,
                            ApprenticeshipType = item.ApprenticeshipType,
                            EffectiveFrom = item.EffectiveFrom,
                            CreatedDateTimeUtc = item.CreatedDateTimeUtc,
                            ModifiedDateTimeUtc = item.ModifiedDateTimeUtc,
                            RecordStatusId = item.RecordStatusId,
                            FrameworkCode = item.FrameworkCode,
                            ProgType = item.ProgType,
                            PathwayCode = item.PathwayCode,
                            PathwayName = item.PathwayName,
                            NasTitle = item.NasTitle,
                            EffectiveTo = item.EffectiveTo,
                            SectorSubjectAreaTier1 = item.SectorSubjectAreaTier1,
                            SectorSubjectAreaTier2 = item.SectorSubjectAreaTier2,
                            NotionalNVQLevelv2 = item.NotionalEndLevel,
                            ProgTypeDesc = item.ProgTypeDesc,
                            ProgTypeDesc2 = item.ProgTypeDesc2,
                            AlreadyCreated = item.AlreadyCreated

                        });
                    }
                }

                model.Items = listOfApprenticeships;
                model.SearchTerm = requestModel.SearchTerm;
            }

            return ViewComponent(nameof(ApprenticeshipSearchResult), model);
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
        public IActionResult Details(DetailViewModel model)
        {
            var ukprn = _session.GetInt32("UKPRN").Value;
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");
            apprenticeship = apprenticeship ?? MapToApprenticeship(model, ukprn, new List<ApprenticeshipLocation>());

            var mode = _session.GetObject<ApprenticeshipMode>("ApprenticeshipMode");
            model.Mode = mode;

            if (model.Mode == ApprenticeshipMode.EditApprenticeship ||
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
        public IActionResult Delivery(DeliveryRequestModel requestModel)
        {
            var model = new DeliveryViewModel();

            return View("../ApprenticeShips/Delivery/Index", model);
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
            LocationChoiceSelectionViewModel model = new LocationChoiceSelectionViewModel();
            if (national == null)
                model.NationalApprenticeship = NationalApprenticeship.Undefined;

            if (national.HasValue)
                model.NationalApprenticeship = national.Value ? NationalApprenticeship.Yes : NationalApprenticeship.No;

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
            
            var cachedLocations = new List<Venue>();
            var locationsResult = await _venueService.SearchAsync(new VenueSearchCriteria(UKPRN.ToString(), ""));
            if (locationsResult.IsSuccess && locationsResult.HasValue)
            {
                cachedLocations = locationsResult.Value.Value.ToList();
            }

            var getApprenticehipByIdResult = await _apprenticeshipService.GetApprenticeshipByIdAsync(requestModel.Id);

            if (getApprenticehipByIdResult.IsSuccess && getApprenticehipByIdResult.HasValue)
            {
                var selectedApprenticeship = getApprenticehipByIdResult.Value;

                model.Apprenticeship = selectedApprenticeship;

                var type = getApprenticehipByIdResult.Value.ApprenticeshipLocations.FirstOrDefault();

                model.Regions = selectedApprenticeship.ApprenticeshipLocations.Any(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased)
                    ? SubRegionCodesToDictionary(selectedApprenticeship.ApprenticeshipLocations.FirstOrDefault(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased)?.Regions) : null;

                model.SummaryOnly = true;
                _session.SetObject("selectedApprenticeship", selectedApprenticeship);

            }

            return View("../Apprenticeships/Summary/Index", model);
        }

        [Authorize]
        public IActionResult Cancel()
        {
            _session.Remove("selectedApprenticeship");
            return RedirectToAction("Index", "ProviderApprenticeships", new { });
        }

        [Authorize]
        public async Task<IActionResult> Summary(SummaryRequestModel requestModel)
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

            var Apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            var model = new SummaryViewModel();
            var mode =_session.GetObject<ApprenticeshipMode>("ApprenticeshipMode");
            


            if (mode == ApprenticeshipMode.EditYourApprenticeships)
            {
                var result = await _apprenticeshipService.UpdateApprenticeshipAsync(Apprenticeship);

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
                Apprenticeship.id = Guid.NewGuid();
                var result = await _apprenticeshipService.AddApprenticeship(Apprenticeship);

                if (result.IsSuccess)
                {
                    return RedirectToAction("Complete", "Apprenticeships");
                }
                else
                {
                    //Action needs to be decided if failure
                    return RedirectToAction("Summary", "Apprenticeships");
                }
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
        public ActionResult AddCombined(AddDeliveryOptionsCombinedModel model)
        {
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            if (model.LocationId.HasValue)
            {
                var venue = _venueService.GetVenueByIdAsync(
                    new GetVenueByIdCriteria(model.LocationId.Value.ToString()));

                string deliveryMethod = string.Empty;

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

                apprenticeship.ApprenticeshipLocations.Add(CreateDeliveryLocation(new DeliveryOption()
                {
                    Delivery = deliveryMethod,
                    LocationId = venue.Result.Value.ID.ToString(),
                    LocationName = venue.Result.Value.VenueName,
                    PostCode = venue.Result.Value.PostCode,
                    Radius = model.Radius,
                    National = model.National,
                    Venue = (Venue)venue.Result.Value

                }, ApprenticeshipLocationType.ClassroomBasedAndEmployerBased));

                _session.SetObject("selectedApprenticeship", apprenticeship);

                return RedirectToAction("DeliveryOptionsCombined", "Apprenticeships");
            }

            return RedirectToAction("Summary", "Apprenticeships");
        }
        
        [HttpPost]
        public ActionResult Add(AddDeliveryOptionViewModel model)
        {
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            if (apprenticeship.ApprenticeshipLocations == null)
            {
                apprenticeship.ApprenticeshipLocations = new List<ApprenticeshipLocation>();
            }

            if (model.LocationId.HasValue)
            {
                var venue = _venueService.GetVenueByIdAsync(
                    new GetVenueByIdCriteria(model.LocationId.Value.ToString()));

                string deliveryMethod = string.Empty;

                if (model.BlockRelease && model.DayRelease)
                {
                    deliveryMethod = "Day release, Block release";
                }
                else
                {
                    deliveryMethod = model.DayRelease ? "Day release" : "Block release";
                }

                apprenticeship.ApprenticeshipLocations.Add(CreateDeliveryLocation(new DeliveryOption()
                {
                    Delivery = deliveryMethod,
                    LocationId = venue.Result.Value.ID.ToString(),
                    LocationName = venue.Result.Value.VenueName,
                    PostCode = venue.Result.Value.PostCode,
                    Venue = (Venue)venue.Result.Value,
                    Radius = "10"

                }, ApprenticeshipLocationType.ClassroomBased));


                _session.SetObject("selectedApprenticeship", apprenticeship);

                return RedirectToAction("DeliveryOptions", "Apprenticeships");
            }
            
            return RedirectToAction("Summary", "Apprenticeships");
        }

        public IActionResult Continue(string LocationId, bool DayRelease, bool BlockRelease, int RowCount)
        {

            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            if (apprenticeship.ApprenticeshipLocations == null)
            {
                apprenticeship.ApprenticeshipLocations = new List<ApprenticeshipLocation>();
            }

            string RadiusValue = "10";

            var venue = _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(LocationId));

            string deliveryMethod = string.Empty;

            if (BlockRelease && DayRelease)
            {
                deliveryMethod = "Day release, Block release";
            }
            else
            {
                deliveryMethod = DayRelease ? "Day release" : "Block release";
            }
            apprenticeship.ApprenticeshipLocations.Add(CreateDeliveryLocation(new DeliveryOption()
            {
                Delivery = deliveryMethod,
                LocationId = venue.Result.Value.ID.ToString(),
                LocationName = venue.Result.Value.VenueName,
                PostCode = venue.Result.Value.PostCode,
                Venue = (Venue)venue.Result.Value,
                Radius = "10"

            }, ApprenticeshipLocationType.ClassroomBased));

            _session.SetObject("selectedApprenticeship", apprenticeship);


            return Json(Url.Action("Summary", "Apprenticeships", new {}));
        }

        public IActionResult ContinueCombined(string LocationId, bool DayRelease, bool BlockRelease, bool National, string Radius, int RowCount)
        {
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            var radiusValue = 0;

            var nationalRadiusValue = _apprenticeshipSettings.Value.NationalRadius;

            radiusValue = National ? nationalRadiusValue : Convert.ToInt32(Radius);

            var venue = _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(LocationId));

            string deliveryMethod = string.Empty;

            if (BlockRelease && DayRelease)
            {
                deliveryMethod = "Employers address, Day release, Block release";
            }
            else
            {
                deliveryMethod = DayRelease ? "Employers address, Day release" : "Employers address, Block release";
            }

            apprenticeship.ApprenticeshipLocations.Add(CreateDeliveryLocation(new DeliveryOption()
            {
                Delivery = deliveryMethod,
                LocationId = venue.Result.Value.ID.ToString(),
                LocationName = venue.Result.Value.VenueName,
                PostCode = venue.Result.Value.PostCode,
                National = National,
                Radius = radiusValue.ToString(),
                Venue = (Venue)venue.Result.Value

            }, ApprenticeshipLocationType.ClassroomBasedAndEmployerBased));

            _session.SetObject("selectedApprenticeship", apprenticeship);
           
            return Json(Url.Action("Summary", "Apprenticeships", new {}));
        }

        [Authorize]
        public IActionResult Complete(CompleteViewModel model)
        {

            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            model.ApprenticeshipName = apprenticeship.ApprenticeshipTitle;

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
        public async Task<IActionResult> WhatWouldYouLIkeToDo(WhatWouldYouLikeToDoViewModel theModel)
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
                    var getApprenticehipByIdResult = await _apprenticeshipService.GetApprenticeshipByIdAsync(theModel.ApprenticeshipId.ToString());

                    if (getApprenticehipByIdResult.IsSuccess && getApprenticehipByIdResult.HasValue)
                    {
                        getApprenticehipByIdResult.Value.RecordStatus = RecordStatus.Deleted;

                        var updateApprenticeshipResult =
                            await _apprenticeshipService.UpdateApprenticeshipAsync(getApprenticehipByIdResult.Value);

                        if (updateApprenticeshipResult.IsSuccess && updateApprenticeshipResult.HasValue)
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
        public async Task<IActionResult> DeleteDeliveryOption(DeleteDeliveryOptionViewModel model)
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
        
        internal Dictionary<string, List<string>> SubRegionCodesToDictionary(IEnumerable<string> subRegions)
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
                apprenticeshipLocation.TribalId = loc.Venue.TribalLocationId ?? null;
                apprenticeshipLocation.ProviderId = loc.Venue.ProviderID;
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
                    Latitude = (double)loc.Venue.Latitude,
                    Longitude = (double)loc.Venue.Longitude
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

        private Apprenticeship MapToApprenticeship(DetailViewModel model, int ukprn, List<ApprenticeshipLocation> locations)
        {
            return new Apprenticeship
            {
                //ApprenticeshipId // For backwards compatibility with Tribal (Where does this come from?)
                //TribalProviderId
                ProviderId = _providerService.GetProviderByPRNAsync(new ProviderSearchCriteria(ukprn.ToString())).Result.Value.Value.FirstOrDefault().id,
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