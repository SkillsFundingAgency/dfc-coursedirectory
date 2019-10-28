﻿using Dfc.CourseDirectory.Common;
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

namespace Dfc.CourseDirectory.Web.Controllers
{
    [Authorize("Apprenticeship")]
    [SelectedProviderNeeded]
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
            _session.Remove("DetailViewModel");
            _session.Remove("DeliveryViewModel");
            _session.Remove("LocationChoiceSelectionViewModel");
            _session.Remove("DeliveryOptions");
            _session.Remove("DeliveryOptionsCombined");
            _session.Remove("RegionsViewModel");
            _session.Remove("SelectedRegions");
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

            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");
            if (apprenticeship != null)
            {
                model = MapToDetailViewModel(apprenticeship);
                model.Mode = request.Mode;
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
                model.Mode = request.Mode;

                model.Cancelled = request.Cancelled.HasValue && request.Cancelled.Value;

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
            // create an apprenticeship Save it to session / db
            // set the mode in session
            var ukprn = _session.GetInt32("UKPRN").Value;
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");
            apprenticeship = apprenticeship ?? MapToApprenticeship(model, ukprn, new List<ApprenticeshipLocation>());




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

                _session.SetObject("selectedApprenticeship", apprenticeship);
            }

            switch (model.Mode)
            {
                case ApprenticeshipMode.Add:
                    if (model.ShowCancelled.HasValue && model.ShowCancelled.Value == true)
                    {
                        return RedirectToAction("Summary", "Apprenticeships", new SummaryRequestModel() { Mode = model.Mode, Id = model.Id.ToString(), cancelled = false });
                    }
                    return RedirectToAction("Delivery", "Apprenticeships", new DeliveryRequestModel() { Mode = model.Mode });
                case ApprenticeshipMode.EditApprenticeship:
                    return RedirectToAction("Summary", "Apprenticeships",
                        new SummaryRequestModel() { Mode = model.Mode, cancelled = false });
                case ApprenticeshipMode.EditYourApprenticeships:
                    return RedirectToAction("Summary", "Apprenticeships", new SummaryRequestModel() { Mode = model.Mode, Id = model.Id.ToString(), cancelled = false });
                default:
                    return RedirectToAction("Delivery", "Apprenticeships", new DeliveryRequestModel() { Mode = model.Mode });

            }
        }


        [Authorize]
        public IActionResult Delivery(DeliveryRequestModel requestModel)
        {
            var model = new DeliveryViewModel();
            model.Mode = requestModel.Mode;


            return View("../ApprenticeShips/Delivery/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Delivery(DeliveryViewModel model)
        {
            switch (model.ApprenticeshipDelivery)
            {
                case ApprenticeshipDelivery.Both:
                    return RedirectToAction("DeliveryOptionsCombined", "Apprenticeships", new { Mode = model.Mode });
                case ApprenticeshipDelivery.EmployersAddress:
                    return RedirectToAction("LocationChoiceSelection", "Apprenticeships", new { Mode = model.Mode });

                case ApprenticeshipDelivery.YourLocation:
                    return RedirectToAction("DeliveryOptions", "Apprenticeships", new { Mode = model.Mode });
                default:
                    return View("../ApprenticeShips/Delivery/Index");

            }

        }

        [Authorize]
        public IActionResult LocationChoiceSelection(ApprenticeshipMode Mode)
        {

            var model = new LocationChoiceSelectionViewModel();

            model.Mode = Mode;
            return View("../Apprenticeships/LocationChoiceSelection/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult LocationChoiceSelection(LocationChoiceSelectionViewModel model)
        {
            switch (model.NationalApprenticeship)
            {
                case NationalApprenticeship.Yes:

                    var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

                    apprenticeship.ApprenticeshipLocations.Add(CreateRegionLocation(new string[0], true));

                    _session.SetObject("selectedApprenticeship", apprenticeship);
                    return RedirectToAction("Summary", "Apprenticeships", new { Mode = model.Mode });

                case NationalApprenticeship.No:
                    return RedirectToAction("Regions", "Apprenticeships", new { Mode = model.Mode });

                default:
                    return View("../ApprenticeShips/Search/Index");

            }
        }

        [Authorize]
        public IActionResult DeliveryOptions(string message, ApprenticeshipMode mode)
        {
            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var model = new DeliveryOptionsViewModel();

            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            model.DeliveryOptionSummary = new DeliveryOptionSummary();
            model.DeliveryOptionSummary.DeliveryOptions = null;
            model.BlockRelease = false;
            model.DayRelease = false;
            model.Radius = null;
            model.locations = apprenticeship?.ApprenticeshipLocations.Where(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBased).ToList();

            ViewBag.Message = message;

            model.Mode = mode;
            return View("../Apprenticeships/DeliveryOptions/Index", model);
        }

        [Authorize]
        public IActionResult DeliveryOptionsCombined(string message, ApprenticeshipMode mode)
        {
            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var model = new DeliveryOptionsCombined();

            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            model.DeliveryOptionSummary = new DeliveryOptionSummary();
            model.DeliveryOptionSummary.DeliveryOptions = null;
            model.BlockRelease = false;
            model.DayRelease = false;
            model.National = false;
            model.Radius = null;
            model.Locations = apprenticeship?.ApprenticeshipLocations.Where(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBasedAndEmployerBased).ToList();


            ViewBag.Message = message;
            model.Mode = mode;
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

                model.Mode = requestModel.Mode;
                model.Apprenticeship = selectedApprenticeship;

                var type = getApprenticehipByIdResult.Value.ApprenticeshipLocations.FirstOrDefault();
                model.Regions = selectedApprenticeship.ApprenticeshipLocations.Any(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased)
                    ? SubRegionCodesToDictionary(selectedApprenticeship.ApprenticeshipLocations.FirstOrDefault(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased)?.Regions) : null;

                model.Mode = requestModel.Mode;

                model.Cancelled = null;
                _session.SetObject("selectedApprenticeship", selectedApprenticeship);

            }

            return View("../Apprenticeships/Summary/Index", model);
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

            var selectedApprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");


            var model = new SummaryViewModel();

            if (selectedApprenticeship != null)
            {
                model.Apprenticeship = selectedApprenticeship;
                model.Regions = model.Regions = selectedApprenticeship.ApprenticeshipLocations.Any(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased)
                    ? SubRegionCodesToDictionary(selectedApprenticeship.ApprenticeshipLocations.FirstOrDefault(x => x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased)?.Regions) : null;

            }

            model.Cancelled = requestModel.cancelled;
            model.Mode = requestModel.Mode;


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


            if (theModel.Mode == ApprenticeshipMode.EditYourApprenticeships)
            {

                if (!theModel.Cancelled.HasValue || theModel.Cancelled.Value == true)
                {
                    return RedirectToAction("Index", "ProviderApprenticeships", new { });
                }

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
        public IActionResult Regions(ApprenticeshipMode Mode)
        {
            var model = new RegionsViewModel();
            model.Mode = Mode;
            model.ChooseRegion = new ChooseRegionModel
            {
                Regions = _courseService.GetRegions(),
                UseNationalComponent = false
            };

            var Apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");
            var employerBased = Apprenticeship?.ApprenticeshipLocations.FirstOrDefault(x =>
                x.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased);
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

            if (employerBased != null)
            {
                employerBased.Regions = SelectedRegions;
            }
            else
            {
                Apprenticeship.ApprenticeshipLocations.Add(CreateRegionLocation(SelectedRegions, null));
            }



            _session.SetObject("selectedApprenticeship", Apprenticeship);

            return RedirectToAction("Summary", "Apprenticeships", new { Mode = model.Mode });

        }

        [HttpPost]
        public ActionResult AddCombined(DeliveryOptionsCombined model)
        {
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            var DeliveryOptionsCombinedViewModel = _session.GetObject<DeliveryOptionsCombined>("DeliveryOptionsCombined");

            if (DeliveryOptionsCombinedViewModel == null)
            {
                DeliveryOptionsCombinedViewModel = model;
            }

            if (DeliveryOptionsCombinedViewModel.DeliveryOptionSummary == null)
            {

                DeliveryOptionsCombinedViewModel.DeliveryOptionSummary = new DeliveryOptionSummary();
                List<DeliveryOption> list = new List<DeliveryOption>();
                DeliveryOptionsCombinedViewModel.DeliveryOptionSummary.DeliveryOptions = list;
            }

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

                DeliveryOptionsCombinedViewModel.DeliveryOptionSummary.DeliveryOptions.Add(
                    new DeliveryOption()
                    {
                        Delivery = deliveryMethod,
                        LocationId = venue.Result.Value.ID.ToString(),
                        LocationName = venue.Result.Value.VenueName,
                        PostCode = venue.Result.Value.PostCode,
                        Radius = model.Radius,
                        National = model.National,
                        Venue = (Venue)venue.Result.Value

                    });

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

                DeliveryOptionsCombinedViewModel.Mode = model.Mode;

                _session.SetObject("DeliveryOptionsCombined", DeliveryOptionsCombinedViewModel);
                _session.SetObject("selectedApprenticeship", apprenticeship);

                return RedirectToAction("DeliveryOptionsCombined", "Apprenticeships", new { Mode = model.Mode });
            }

            _session.SetObject("DeliveryOptionsCombined", DeliveryOptionsCombinedViewModel);

            return RedirectToAction("Summary", "Apprenticeships", new { Mode = model.Mode });
        }

        [Authorize]
        public IActionResult Delete(string locationid, ApprenticeshipMode Mode)
        {
            var deliveryOptionsViewModel = _session.GetObject<DeliveryOptionsViewModel>("DeliveryOptions");

            var itemToRemove = deliveryOptionsViewModel.DeliveryOptionSummary.DeliveryOptions.FirstOrDefault(x => x.LocationId == locationid);

            if (itemToRemove != null)
            {
                deliveryOptionsViewModel.DeliveryOptionSummary.DeliveryOptions.Remove(itemToRemove);
            }

            deliveryOptionsViewModel.BlockRelease = false;
            deliveryOptionsViewModel.DayRelease = false;
            deliveryOptionsViewModel.LocationId = null;
            deliveryOptionsViewModel.National = false;
            deliveryOptionsViewModel.Mode = Mode;
            locationid = "";
            _session.SetObject("DeliveryOptions", deliveryOptionsViewModel);

            return RedirectToAction("DeliveryOptions", "Apprenticeships");
        }

        [Authorize]
        public IActionResult DeleteCombined(string locationid, ApprenticeshipMode Mode)
        {
            var deliveryOptionsCombinedViewModel = _session.GetObject<DeliveryOptionsCombined>("DeliveryOptionsCombined");

            var itemToRemove = deliveryOptionsCombinedViewModel.DeliveryOptionSummary.DeliveryOptions.FirstOrDefault(x => x.LocationId == locationid);

            if (itemToRemove != null)
            {
                deliveryOptionsCombinedViewModel.DeliveryOptionSummary.DeliveryOptions.Remove(itemToRemove);
            }

            deliveryOptionsCombinedViewModel.BlockRelease = false;
            deliveryOptionsCombinedViewModel.DayRelease = false;
            deliveryOptionsCombinedViewModel.LocationId = null;
            deliveryOptionsCombinedViewModel.National = false;

            deliveryOptionsCombinedViewModel.Mode = Mode;
            locationid = "";
            _session.SetObject("DeliveryOptionsCombined", deliveryOptionsCombinedViewModel);

            // return View("../ApprenticeshipDeliveryOptions/Index", apprenticeshipDeliveryOptionsViewModel);
            return RedirectToAction("DeliveryOptionsCombined", "Apprenticeships");
        }

        [HttpPost]
        public ActionResult Add(DeliveryOptionsViewModel model)
        {
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            if (apprenticeship.ApprenticeshipLocations == null)
            {
                apprenticeship.ApprenticeshipLocations = new List<ApprenticeshipLocation>();
            }


            var DeliveryOptionsViewModel = _session.GetObject<DeliveryOptionsViewModel>("DeliveryOptions");

            if (DeliveryOptionsViewModel == null)
            {
                DeliveryOptionsViewModel = model;
            }

            if (DeliveryOptionsViewModel.DeliveryOptionSummary == null)
            {

                DeliveryOptionsViewModel.DeliveryOptionSummary = new DeliveryOptionSummary();
                List<DeliveryOption> list = new List<DeliveryOption>();
                DeliveryOptionsViewModel.DeliveryOptionSummary.DeliveryOptions = list;
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



                DeliveryOptionsViewModel.DeliveryOptionSummary.DeliveryOptions.Add(
                    new DeliveryOption()
                    {
                        Delivery = deliveryMethod,
                        LocationId = venue.Result.Value.ID.ToString(),
                        LocationName = venue.Result.Value.VenueName,
                        PostCode = venue.Result.Value.PostCode,
                        Venue = (Venue)venue.Result.Value,
                        Radius = "10"

                    });


                apprenticeship.ApprenticeshipLocations.Add(CreateDeliveryLocation(new DeliveryOption()
                {
                    Delivery = deliveryMethod,
                    LocationId = venue.Result.Value.ID.ToString(),
                    LocationName = venue.Result.Value.VenueName,
                    PostCode = venue.Result.Value.PostCode,
                    Venue = (Venue)venue.Result.Value,
                    Radius = "10"

                }, ApprenticeshipLocationType.ClassroomBased));

                DeliveryOptionsViewModel.Mode = model.Mode;

                _session.SetObject("DeliveryOptions", DeliveryOptionsViewModel);
                _session.SetObject("selectedApprenticeship", apprenticeship);

                return RedirectToAction("DeliveryOptions", "Apprenticeships", new { Mode = model.Mode });
            }

            _session.SetObject("DeliveryOptions", DeliveryOptionsViewModel);

            return RedirectToAction("Summary", "Apprenticeships", new { Mode = model.Mode });
        }

        public IActionResult Continue(string LocationId, bool DayRelease, bool BlockRelease, int RowCount, ApprenticeshipMode Mode)
        {

            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            if (apprenticeship.ApprenticeshipLocations == null)
            {
                apprenticeship.ApprenticeshipLocations = new List<ApprenticeshipLocation>();
            }

            string RadiusValue = "10";
            var DeliveryOptionsViewModel = _session.GetObject<DeliveryOptionsViewModel>("DeliveryOptions");

            if (DeliveryOptionsViewModel == null)
            {
                DeliveryOptionsViewModel = new DeliveryOptionsViewModel();
                DeliveryOptionsViewModel.Mode = Mode;
            }

            if (DeliveryOptionsViewModel.DeliveryOptionSummary == null)
            {

                DeliveryOptionsViewModel.DeliveryOptionSummary = new DeliveryOptionSummary();
                List<DeliveryOption> list = new List<DeliveryOption>();
                DeliveryOptionsViewModel.DeliveryOptionSummary.DeliveryOptions = list;
            }

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

            DeliveryOptionsViewModel.DeliveryOptionSummary.DeliveryOptions.Add(new DeliveryOption()
            {
                Delivery = deliveryMethod,
                LocationId = venue.Result.Value.ID.ToString(),
                LocationName = venue.Result.Value.VenueName,
                PostCode = venue.Result.Value.PostCode,
                Radius = RadiusValue,
                Venue = (Venue)venue.Result.Value

            });

            apprenticeship.ApprenticeshipLocations.Add(CreateDeliveryLocation(new DeliveryOption()
            {
                Delivery = deliveryMethod,
                LocationId = venue.Result.Value.ID.ToString(),
                LocationName = venue.Result.Value.VenueName,
                PostCode = venue.Result.Value.PostCode,
                Venue = (Venue)venue.Result.Value,
                Radius = "10"

            }, ApprenticeshipLocationType.ClassroomBased));

            _session.SetObject("DeliveryOptions", DeliveryOptionsViewModel);
            _session.SetObject("selectedApprenticeship", apprenticeship);


            return Json(Url.Action("Summary", "Apprenticeships", new { Mode = Mode }));
        }

        public IActionResult ContinueCombined(string LocationId, bool DayRelease, bool BlockRelease, bool National, string Radius, int RowCount, ApprenticeshipMode Mode)
        {
            var apprenticeship = _session.GetObject<Apprenticeship>("selectedApprenticeship");

            var RadiusValue = 0;

            var nationalRadiusValue = _apprenticeshipSettings.Value.NationalRadius;

            RadiusValue = National ? nationalRadiusValue : Convert.ToInt32(Radius);

            var DeliveryOptionsCombinedViewModel = _session.GetObject<DeliveryOptionsCombined>("DeliveryOptionsCombined") ??
                                                                 new DeliveryOptionsCombined();

            if (DeliveryOptionsCombinedViewModel.DeliveryOptionSummary == null)
            {
                DeliveryOptionsCombinedViewModel.Mode = Mode;
                DeliveryOptionsCombinedViewModel.DeliveryOptionSummary = new DeliveryOptionSummary();
                List<DeliveryOption> list = new List<DeliveryOption>();
                DeliveryOptionsCombinedViewModel.DeliveryOptionSummary.DeliveryOptions = list;
            }

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

            DeliveryOptionsCombinedViewModel.DeliveryOptionSummary.DeliveryOptions.Add(new DeliveryOption()
            {
                Delivery = deliveryMethod,
                LocationId = venue.Result.Value.ID.ToString(),
                LocationName = venue.Result.Value.VenueName,
                PostCode = venue.Result.Value.PostCode,
                National = National,
                Radius = RadiusValue.ToString(),
                Venue = (Venue)venue.Result.Value

            });

            apprenticeship.ApprenticeshipLocations.Add(CreateDeliveryLocation(new DeliveryOption()
            {
                Delivery = deliveryMethod,
                LocationId = venue.Result.Value.ID.ToString(),
                LocationName = venue.Result.Value.VenueName,
                PostCode = venue.Result.Value.PostCode,
                National = National,
                Radius = RadiusValue.ToString(),
                Venue = (Venue)venue.Result.Value

            }, ApprenticeshipLocationType.ClassroomBasedAndEmployerBased));

            if (Mode != ApprenticeshipMode.Undefined)
            {
                DeliveryOptionsCombinedViewModel.Mode = Mode;
            }
            _session.SetObject("DeliveryOptionsCombined", DeliveryOptionsCombinedViewModel);
            _session.SetObject("selectedApprenticeship", apprenticeship);
            //  }
            return Json(Url.Action("Summary", "Apprenticeships", new { Mode = Mode }));
        }

        [Authorize]
        public IActionResult Complete(CompleteViewModel model)
        {

            var DetailViewModel = _session.GetObject<DetailViewModel>("DetailViewModel");

            model.ApprenticeshipName = DetailViewModel.ApprenticeshipTitle;

            _session.Remove("DetailViewModel");
            _session.Remove("DeliveryViewModel");
            _session.Remove("LocationChoiceSelectionViewModel");
            _session.Remove("DeliveryOptions");
            _session.Remove("DeliveryOptionsCombined");
            _session.Remove("RegionsViewModel");
            _session.Remove("SelectedRegions");
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
        public IActionResult DeleteDeliveryOption(string LocationName, ApprenticeshipMode Mode)
        {
            var model = new DeleteDeliveryOptionViewModel();

            model.Combined = false;
            model.LocationName = LocationName;
            model.Mode = Mode;

            var DetailViewModel = _session.GetObject<DetailViewModel>("DetailViewModel");
            var DeliveryOptionsCombinedViewModel = _session.GetObject<DeliveryOptionsCombined>("DeliveryOptionsCombined");

            if (DeliveryOptionsCombinedViewModel != null)
            {
                if (DeliveryOptionsCombinedViewModel.DeliveryOptionSummary != null)
                {
                    model.Combined = true;
                }
            }

            model.ApprenticeshipTitle = DetailViewModel.ApprenticeshipTitle;

            return View("../Apprenticeships/DeleteDeliveryOption/Index", model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteDeliveryOption(DeleteDeliveryOptionViewModel model)
        {
            if (model.Combined)
            {
                var DeliveryOptionsCombinedViewModel = _session.GetObject<DeliveryOptionsCombined>("DeliveryOptionsCombined");

                var item = DeliveryOptionsCombinedViewModel.DeliveryOptionSummary.DeliveryOptions
                    .SingleOrDefault(x => x.LocationName == model.LocationName);
                DeliveryOptionsCombinedViewModel.DeliveryOptionSummary.DeliveryOptions.Remove(item);

                _session.SetObject("DeliveryOptionsCombined", DeliveryOptionsCombinedViewModel);
            }
            else
            {
                var DeliveryOptionsViewModel = _session.GetObject<DeliveryOptionsViewModel>("DeliveryOptions");

                var item = DeliveryOptionsViewModel.DeliveryOptionSummary.DeliveryOptions
                    .SingleOrDefault(x => x.LocationName == model.LocationName);
                DeliveryOptionsViewModel.DeliveryOptionSummary.DeliveryOptions.Remove(item);

                _session.SetObject("DeliveryOptions", DeliveryOptionsViewModel);
            }

            return RedirectToAction(model.Combined ? "DeliveryOptionsCombined" : "DeliveryOptions", "Apprenticeships", new { message = "Location " + model.LocationName + " deleted", mode = model.Mode });
        }

        [Authorize]

        public IActionResult AddNewVenue(AddCourseRequestModel model)
        {
            _session.SetString("Option", "AddNewVenueForApprenticeships");
            var DeliveryOptionsViewModel = _session.GetObject<DeliveryOptionsViewModel>("DeliveryOptionsViewModel");

            _session.SetObject("DeliveryOptionsViewModel", DeliveryOptionsViewModel);

            return Json(Url.Action("AddVenue", "Venues"));
        }

        internal Dictionary<string, List<string>> SubRegionCodesToDictionary(IEnumerable<string> subRegions)
        {
            SelectRegionModel selectRegionModel = new SelectRegionModel();
            Dictionary<string, List<string>> regionsAndSubregions = new Dictionary<string, List<string>>();

            foreach (var subRegionCode in subRegions)
            {
                var regionName = selectRegionModel.GetRegionNameForSubRegion(subRegionCode);
                if (!string.IsNullOrWhiteSpace(regionName))
                {
                    if (!regionsAndSubregions.ContainsKey(regionName))
                    {
                        regionsAndSubregions.Add(regionName, new List<string>());
                    }
                    var subRegionItem = selectRegionModel.GetSubRegionItemByRegionCode(subRegionCode);
                    regionsAndSubregions[regionName].Add(subRegionItem.SubRegionName);
                }

            }
            return regionsAndSubregions;
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

        private DetailViewModel MapToDetailViewModel(Apprenticeship apprenticeship)
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
                NotionalNVQLevelv2 = apprenticeship.NotionalNVQLevelv2
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