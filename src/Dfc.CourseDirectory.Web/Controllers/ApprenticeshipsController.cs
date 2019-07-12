using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Interfaces.Venues;
using Dfc.CourseDirectory.Models.Models.Venues;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using Microsoft.EntityFrameworkCore;

namespace Dfc.CourseDirectory.Web.Controllers
{
    [Authorize("Apprenticeship")]
    public class ApprenticeshipsController : Controller
    {
        private readonly ILogger<ApprenticeshipsController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;
        private readonly IApprenticeshipService _apprenticeshipService;

        public ApprenticeshipsController(
            ILogger<ApprenticeshipsController> logger,
            IHttpContextAccessor contextAccessor, ICourseService courseService, IVenueService venueService
            , IApprenticeshipService apprenticeshipService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(venueService, nameof(venueService));
            Throw.IfNull(apprenticeshipService, nameof(apprenticeshipService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _venueService = venueService;
            _apprenticeshipService = apprenticeshipService;
        }

        [Authorize]
        public IActionResult Index()
        {
            _session.Remove("DetailViewModel");
            _session.Remove("DeliveryViewModel");
            _session.Remove("LocationChoiceSelectionViewModel");
            _session.Remove("DeliveryOptionsViewModel");
            _session.Remove("DeliveryOptionsCombinedViewModel");
            _session.Remove("RegionsViewModel");
            return View("../ApprenticeShips/Search/Index");
        }


        [Authorize]
        public async Task<IActionResult> Search([FromQuery] SearchRequestModel requestModel)
        {
            ApprenticeshipsSearchResultModel model = new ApprenticeshipsSearchResultModel();

            if (!string.IsNullOrEmpty(requestModel.SearchTerm))
            {
                var result = await _apprenticeshipService.StandardsAndFrameworksSearch(requestModel.SearchTerm);

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
                            NotionalEndLevel = item.NotionalEndLevel,
                            ProgTypeDesc = item.ProgTypeDesc,
                            ProgTypeDesc2 = item.ProgTypeDesc2,

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

            var DetailViewModel = _session.GetObject<DetailViewModel>("DetailViewModel");
            if (DetailViewModel != null)
            {
                model = DetailViewModel;
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
                model.Version = request.Version;
                model.NotionalEndLevel = request.NotionalEndLevel;
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
            _session.SetObject("DetailViewModel", model);

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

            var DeliveryViewModel = _session.GetObject<DeliveryViewModel>("DeliveryViewModel");
            if (DeliveryViewModel != null)
            {
                model = DeliveryViewModel;
            }

            return View("../ApprenticeShips/Delivery/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Delivery(DeliveryViewModel model)
        {
            _session.SetObject("DeliveryViewModel", model);

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

            var LocationChoiceSelectionViewModel = _session.GetObject<LocationChoiceSelectionViewModel>("LocationChoiceSelectionViewModel");
            if (LocationChoiceSelectionViewModel != null)
            {
                model = LocationChoiceSelectionViewModel;
            }

            return View("../Apprenticeships/LocationChoiceSelection/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult LocationChoiceSelection(LocationChoiceSelectionViewModel model)
        {
            _session.SetObject("LocationChoiceSelectionViewModel", model);
            switch (model.NationalApprenticeship)
            {
                case NationalApprenticeship.Yes:
                    return RedirectToAction("Summary", "Apprenticeships", new { ApprenticeshipMode = model.Mode });

                case NationalApprenticeship.No:
                    return RedirectToAction("Regions", "Apprenticeships", new { ApprenticeshipMode = model.Mode });

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

            var DeliveryOptionsViewModel = _session.GetObject<DeliveryOptionsViewModel>("DeliveryOptionsViewModel");
            if (DeliveryOptionsViewModel != null)
            {
                model = DeliveryOptionsViewModel;
                model.BlockRelease = false;
                model.DayRelease = false;
                model.Radius = null;
            }
            else
            {
                model.DeliveryOptionsListItemModel = new DeliveryOptionsListModel();
                model.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel = null;
                model.BlockRelease = false;
                model.DayRelease = false;
                model.Radius = null;
            }

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

            var model = new DeliveryOptionsCombinedViewModel();

            var DeliveryOptionsCombinedViewModel = _session.GetObject<DeliveryOptionsCombinedViewModel>("DeliveryOptionsCombinedViewModel");
            if (DeliveryOptionsCombinedViewModel != null)
            {
                model = DeliveryOptionsCombinedViewModel;
                model.BlockRelease = false;
                model.DayRelease = false;
                model.National = false;
                model.Radius = null;
            }
            else
            {
                model.DeliveryOptionsListItemModel = new DeliveryOptionsListModel();
                model.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel = null;
                model.BlockRelease = false;
                model.DayRelease = false;
                model.National = false;
                model.Radius = null;
            }

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

                var DetailViewModel = new DetailViewModel();
                var DeliveryViewModel = new DeliveryViewModel();
                var LocationChoiceSelectionViewModel = new LocationChoiceSelectionViewModel();

                //var Regions = new String[];

                DetailViewModel.ApprenticeshipTitle = getApprenticehipByIdResult.Value.ApprenticeshipTitle;
                DetailViewModel.ApprenticeshipType = getApprenticehipByIdResult.Value.ApprenticeshipType;
                DetailViewModel.ContactUsIUrl = getApprenticehipByIdResult.Value.ContactWebsite;
                DetailViewModel.Email = getApprenticehipByIdResult.Value.ContactEmail;
                DetailViewModel.FrameworkCode = getApprenticehipByIdResult.Value.FrameworkCode;
                DetailViewModel.Id = getApprenticehipByIdResult.Value.id;
                DetailViewModel.Information = getApprenticehipByIdResult.Value.MarketingInformation;
                DetailViewModel.NotionalEndLevel = getApprenticehipByIdResult.Value.NotionalEndLevel;
                DetailViewModel.StandardCode = getApprenticehipByIdResult.Value.StandardCode;
                DetailViewModel.Telephone = getApprenticehipByIdResult.Value.ContactTelephone;
                DetailViewModel.Website = getApprenticehipByIdResult.Value.ContactWebsite;
                DetailViewModel.Mode = requestModel.Mode;
                model.DetailViewModel = DetailViewModel;



                model.DeliveryViewModel = DeliveryViewModel;

                var type = getApprenticehipByIdResult.Value.ApprenticeshipLocations.FirstOrDefault();
                if (type.ApprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBased)
                {
                    model.DeliveryOptionsViewModel = new DeliveryOptionsViewModel();
                    DeliveryViewModel.ApprenticeshipDelivery = ApprenticeshipDelivery.YourLocation;
                    DeliveryOptionsListModel deliveryOptionsListModel = new DeliveryOptionsListModel();
                    DeliveryOptionsViewModel deliveryOptionsViewModel = new DeliveryOptionsViewModel();
                    deliveryOptionsListModel.DeliveryOptionsListItemModel = new List<DeliveryOptionsListItemModel>();

                    deliveryOptionsViewModel.DeliveryOptionsListItemModel = deliveryOptionsListModel;
                    model.DeliveryOptionsViewModel = deliveryOptionsViewModel;
                    model.DeliveryOptionsViewModel.Mode = requestModel.Mode;
                }

                if (type.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased)
                {
                    DeliveryViewModel.ApprenticeshipDelivery = ApprenticeshipDelivery.EmployersAddress;
                    DeliveryViewModel.Mode = requestModel.Mode;
                }

                if (type.ApprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBasedAndEmployerBased)
                {
                    model.DeliveryOptionsCombinedViewModel = new DeliveryOptionsCombinedViewModel();
                    model.DeliveryOptionsCombinedViewModel.Mode = requestModel.Mode;
                    DeliveryViewModel.ApprenticeshipDelivery = ApprenticeshipDelivery.Both;
                    DeliveryOptionsListModel deliveryOptionsListModel = new DeliveryOptionsListModel();
                    DeliveryOptionsCombinedViewModel deliveryOptionsCombinedViewModel = new DeliveryOptionsCombinedViewModel();
                    deliveryOptionsListModel.DeliveryOptionsListItemModel = new List<DeliveryOptionsListItemModel>();

                    deliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel = deliveryOptionsListModel;

                    model.DeliveryOptionsCombinedViewModel = deliveryOptionsCombinedViewModel;
                }

                foreach (var loc in getApprenticehipByIdResult.Value.ApprenticeshipLocations)
                {
                    var deliveryOptionsListItemModel = new DeliveryOptionsListItemModel();


                    string delModes = string.Empty;

                    foreach (int deliveryMode in loc.DeliveryModes)
                    {
                        switch (deliveryMode)
                        {
                            case 0:

                                delModes = string.Join(",", @WebHelper.GetEnumDescription(ApprenticeShipDeliveryLocation.Undefined));
                                break;
                            case 1:
                                delModes = string.Join(",", @WebHelper.GetEnumDescription(ApprenticeShipDeliveryLocation.BlockRelease));
                                break;
                            case 2:
                                delModes = string.Join(",", @WebHelper.GetEnumDescription(ApprenticeShipDeliveryLocation.DayRelease));
                                break;
                            default:
                                break;
                        }

                    }

                    deliveryOptionsListItemModel.Delivery = delModes;

                    switch (loc.ApprenticeshipLocationType)
                    {
                        case ApprenticeshipLocationType.ClassroomBased:
                            model.DeliveryOptionsViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel
                                .Add(deliveryOptionsListItemModel);
                            break;
                        case ApprenticeshipLocationType.ClassroomBasedAndEmployerBased:

                            deliveryOptionsListItemModel.LocationId = loc.LocationGuidId.ToString();
                            deliveryOptionsListItemModel.LocationName = cachedLocations.Where(x => x.ID == loc.LocationGuidId.ToString()).Select(x => x.VenueName).FirstOrDefault();
                            deliveryOptionsListItemModel.Radius = loc.Radius.ToString();
                            deliveryOptionsListItemModel.PostCode = cachedLocations.Where(x => x.ID == loc.LocationGuidId.ToString()).Select(x => x.PostCode).FirstOrDefault();
                            model.DeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel
                                .Add(deliveryOptionsListItemModel);
                            break;
                        case ApprenticeshipLocationType.EmployerBased:
                            break;
                    }

                }

                // model.Regions = Regions;
                model.LocationChoiceSelectionViewModel = LocationChoiceSelectionViewModel;

                model.Mode = requestModel.Mode;

                model.Cancelled = null;

                _session.SetObject("DetailViewModel", model.DetailViewModel);
                _session.SetObject("DeliveryViewModel", model.DeliveryViewModel);
                _session.SetObject("LocationChoiceSelectionViewModel", model.LocationChoiceSelectionViewModel);
                _session.SetObject("DeliveryOptionsViewModel", model.DeliveryViewModel);
                _session.SetObject("DeliveryOptionsCombinedViewModel", model.DeliveryOptionsCombinedViewModel);
                //_session.SetObject<String[]>("SelectedRegions");

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

            //if (requestModel.apprenticeshipMode == ApprenticeshipMode.Undefined)
            //{
            //    requestModel.apprenticeshipMode = ApprenticeshipMode.EditApprenticeship;
            //}

            var model = new SummaryViewModel();

            var cachedLocations = new List<Venue>();
            var locationsResult = await _venueService.SearchAsync(new VenueSearchCriteria(UKPRN.ToString(), ""));
            if (locationsResult.IsSuccess && locationsResult.HasValue)
            {
                cachedLocations = locationsResult.Value.Value.ToList();
            }

            var DetailViewModel = _session.GetObject<DetailViewModel>("DetailViewModel");
            var DeliveryViewModel = _session.GetObject<DeliveryViewModel>("DeliveryViewModel");
            var LocationChoiceSelectionViewModel = _session.GetObject<LocationChoiceSelectionViewModel>("LocationChoiceSelectionViewModel");
            var DeliveryOptionsViewModel = _session.GetObject<DeliveryOptionsViewModel>("DeliveryOptionsViewModel");
            var DeliveryOptionsCombinedViewModel = _session.GetObject<DeliveryOptionsCombinedViewModel>("DeliveryOptionsCombinedViewModel");
            var Regions = _session.GetObject<String[]>("SelectedRegions");

            model.DetailViewModel = DetailViewModel;
            model.DeliveryViewModel = DeliveryViewModel;
            model.DeliveryOptionsViewModel = DeliveryOptionsViewModel;
            model.DeliveryOptionsCombinedViewModel = DeliveryOptionsCombinedViewModel;
            model.Regions = Regions;
            model.LocationChoiceSelectionViewModel = LocationChoiceSelectionViewModel;

            //if (requestModel.apprenticeshipMode == ApprenticeshipMode.Undefined)
            //{
            //    model.Mode = DetailViewModel.Mode;
            //}
            //else
            //{
            //    model.Mode = requestModel.apprenticeshipMode;

            //}

            model.Cancelled = requestModel.cancelled;
            model.Mode = requestModel.Mode;
            //}

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

            var model = new SummaryViewModel();

            var DetailViewModel = _session.GetObject<DetailViewModel>("DetailViewModel");
            var DeliveryViewModel = _session.GetObject<DeliveryViewModel>("DeliveryViewModel");
            var LocationChoiceSelectionViewModel = _session.GetObject<LocationChoiceSelectionViewModel>("LocationChoiceSelectionViewModel");
            var DeliveryOptionsViewModel = _session.GetObject<DeliveryOptionsViewModel>("DeliveryOptionsViewModel");
            var DeliveryOptionsCombinedViewModel = _session.GetObject<DeliveryOptionsCombinedViewModel>("DeliveryOptionsCombinedViewModel");
            var Regions = _session.GetObject<String[]>("SelectedRegions");

            model.DetailViewModel = DetailViewModel;

            model.DeliveryViewModel = DeliveryViewModel;

            model.DeliveryOptionsViewModel = DeliveryOptionsViewModel;

            model.DeliveryOptionsCombinedViewModel = DeliveryOptionsCombinedViewModel;

            model.Regions = Regions ?? new string[0];

            model.LocationChoiceSelectionViewModel = LocationChoiceSelectionViewModel;

            ApprenticeshipLocationType apprenticeshipLocationType = new ApprenticeshipLocationType();

            switch (model.DeliveryViewModel.ApprenticeshipDelivery)
            {
                case ApprenticeshipDelivery.YourLocation:
                    apprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBased;
                    break;
                case ApprenticeshipDelivery.EmployersAddress:
                    apprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased;
                    break;
                case ApprenticeshipDelivery.Both:
                    apprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBasedAndEmployerBased;
                    break;
            }

            List<ApprenticeshipLocation> locations = new List<ApprenticeshipLocation>();

            if (model.DeliveryOptionsViewModel?.DeliveryOptionsListItemModel != null)
            {
                foreach (var loc in model.DeliveryOptionsViewModel.DeliveryOptionsListItemModel
                    .DeliveryOptionsListItemModel)
                {
                    List<int> deliveryModes = new List<int>();

                    ApprenticeshipLocation apprenticeshipLocation = new ApprenticeshipLocation()
                    {
                        CreatedDate = DateTime.Now,
                        CreatedBy =
                            User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault(),
                        ApprenticeshipLocationType = apprenticeshipLocationType,
                        id = Guid.NewGuid(),
                        LocationType = LocationType.Venue,
                        RecordStatus = RecordStatus.Live,
                        National = null


                    };

                    if (!string.IsNullOrEmpty(loc.LocationId))
                    {
                        apprenticeshipLocation.LocationGuidId = new Guid(loc.LocationId);
                    }

                    if (!string.IsNullOrEmpty(loc.Radius))
                    {
                        apprenticeshipLocation.Radius = Convert.ToInt32(loc.Radius);
                    }

                    var delModes = loc.Delivery.Split(",");
                    foreach (var delMode in delModes)
                    {
                        if (delMode.ToLower() ==
                            @WebHelper.GetEnumDescription(ApprenticeShipDeliveryLocation.DayRelease).ToLower())
                        {
                            deliveryModes.Add((int)ApprenticeShipDeliveryLocation.DayRelease);
                        }

                        if (delMode.ToLower() == @WebHelper
                                .GetEnumDescription(ApprenticeShipDeliveryLocation.BlockRelease).ToLower())
                        {
                            deliveryModes.Add((int)ApprenticeShipDeliveryLocation.BlockRelease);
                        }
                    }

                    apprenticeshipLocation.DeliveryModes = deliveryModes;

                    locations.Add(apprenticeshipLocation);
                }
            }


            if (model.DeliveryOptionsCombinedViewModel?.DeliveryOptionsListItemModel != null)
            {
                foreach (var loc in model.DeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel
                    .DeliveryOptionsListItemModel)
                {
                    List<int> deliveryModes = new List<int>();

                    ApprenticeshipLocation apprenticeshipLocation = new ApprenticeshipLocation()
                    {
                        CreatedDate = DateTime.Now,
                        CreatedBy =
                            User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault(),
                        ApprenticeshipLocationType = apprenticeshipLocationType,
                        id = Guid.NewGuid(),
                        LocationType = LocationType.Venue,
                        RecordStatus = RecordStatus.Live,
                        National = loc.National != null && loc.National.Value
                    };

                    if (!string.IsNullOrEmpty(loc.LocationId))
                    {
                        apprenticeshipLocation.LocationGuidId = new Guid(loc.LocationId);
                    }

                    if (!string.IsNullOrEmpty(loc.Radius))
                    {
                        apprenticeshipLocation.Radius = Convert.ToInt32(loc.Radius);
                    }

                    var delModes = loc.Delivery.Split(",");
                    foreach (var delMode in delModes)
                    {
                        if (delMode.ToLower() ==
                            @WebHelper.GetEnumDescription(ApprenticeShipDeliveryLocation.DayRelease).ToLower())
                        {
                            deliveryModes.Add((int)ApprenticeShipDeliveryLocation.DayRelease);
                        }

                        if (delMode.ToLower() == @WebHelper
                                .GetEnumDescription(ApprenticeShipDeliveryLocation.BlockRelease).ToLower())
                        {
                            deliveryModes.Add((int)ApprenticeShipDeliveryLocation.BlockRelease);
                        }
                    }

                    apprenticeshipLocation.DeliveryModes = deliveryModes;

                    locations.Add(apprenticeshipLocation);
                }
            }


            Apprenticeship apprenticeship = new Apprenticeship
            {
                //ApprenticeshipId // For backwards compatibility with Tribal (Where does this come from?)
                //TribalProviderId // For backwards compatibility with Tribal (Where does this come from?)#
                //ProviderId // Is this from our Provider collection?
                //id = Guid.NewGuid(),
                ProviderUKPRN = UKPRN,
                ApprenticeshipTitle = model.DetailViewModel.ApprenticeshipTitle,
                ApprenticeshipType = model.DetailViewModel.ApprenticeshipType,
                StandardCode = model.DetailViewModel.StandardCode,
                FrameworkCode = model.DetailViewModel.FrameworkCode,
                ProgType = model.DetailViewModel.ProgType,
                MarketingInformation = model.DetailViewModel.Information,
                Url = model.DetailViewModel.Website,
                ContactTelephone = model.DetailViewModel.Telephone,
                ContactEmail = model.DetailViewModel.Email,
                ContactWebsite = model.DetailViewModel.ContactUsIUrl,
                CreatedDate = DateTime.Now,
                CreatedBy = User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault(),
                RecordStatus = RecordStatus.Live,
                PathwayCode = model.DetailViewModel.PathwayCode,
                Version = model.DetailViewModel.Version,
                NotionalEndLevel = model.DetailViewModel.NotionalEndLevel,
                ApprenticeshipLocations = locations
            };

            switch (apprenticeship.ApprenticeshipType)
            {
                case ApprenticeshipType.StandardCode:
                    {
                        apprenticeship.StandardId = model.DetailViewModel.Id;
                        break;
                    }

                case ApprenticeshipType.FrameworkCode:
                    {
                        apprenticeship.FrameworkId = model.DetailViewModel.Id;
                        break;
                    }
            }

            if (theModel.Mode == ApprenticeshipMode.EditYourApprenticeships)
            {

                if (!theModel.Cancelled.HasValue || theModel.Cancelled.Value == true)
                {
                    return RedirectToAction("Index", "ProviderApprenticeships", new { });
                }

                apprenticeship.id = DetailViewModel.Id;
                apprenticeship.UpdatedDate = DateTime.UtcNow;
                apprenticeship.UpdatedBy =
                    User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault();
                var result = await _apprenticeshipService.UpdateApprenticeshipAsync(apprenticeship);

                if (result.IsSuccess)
                {
                    return RedirectToAction("Index", "ProviderApprenticeships", new { });
                }
                else
                {
                    //Action needs to be decided if failure
                    return RedirectToAction("Summary", "Apprenticeships");
                }

            }
            else
            {
                apprenticeship.id = Guid.NewGuid();
                var result = await _apprenticeshipService.AddApprenticeship(apprenticeship);

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


            var SelectedRegions = _session.GetObject<string[]>("SelectedRegions");
            if (SelectedRegions != null)
            {



                foreach (var selectRegionRegionItem in model.ChooseRegion.Regions.RegionItems.OrderBy(x => x.RegionName))
                {
                    foreach (var subRegionItemModel in selectRegionRegionItem.SubRegion)
                    {
                        if (SelectedRegions.Contains(subRegionItemModel.Id))
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
            _session.SetObject("SelectedRegions", SelectedRegions);

            return RedirectToAction("Summary", "Apprenticeships", new { ApprenticeshipMode = model.Mode });


        }

        [HttpPost]
        public ActionResult AddCombined(DeliveryOptionsCombinedViewModel model)
        {
            var DeliveryOptionsCombinedViewModel = _session.GetObject<DeliveryOptionsCombinedViewModel>("DeliveryOptionsCombinedViewModel");

            if (DeliveryOptionsCombinedViewModel == null)
            {
                DeliveryOptionsCombinedViewModel = model;
            }

            if (DeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel == null)
            {

                DeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel = new DeliveryOptionsListModel();
                List<DeliveryOptionsListItemModel> list = new List<DeliveryOptionsListItemModel>();
                DeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel = list;
            }

            var venue = _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(model.LocationId.Value.ToString()));

            string deliveryMethod = string.Empty;

            if (model.BlockRelease && model.DayRelease)
            {
                deliveryMethod = "Day release, Block release";
            }
            else
            {
                deliveryMethod = model.DayRelease ? "Day release" : "Block release";
            }

            DeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.Add(new DeliveryOptionsListItemModel()
            {
                Delivery = deliveryMethod,
                LocationId = venue.Result.Value.ID.ToString(),
                LocationName = venue.Result.Value.VenueName,
                PostCode = venue.Result.Value.PostCode,
                Radius = model.Radius,
                National = model.National

            });

            DeliveryOptionsCombinedViewModel.Mode = model.Mode;
            _session.SetObject("DeliveryOptionsCombinedViewModel", DeliveryOptionsCombinedViewModel);

            return RedirectToAction("DeliveryOptionsCombined", "Apprenticeships", new { Mode = model.Mode });
        }

        [Authorize]
        public IActionResult Delete(string locationid, ApprenticeshipMode Mode)
        {
            var deliveryOptionsViewModel = _session.GetObject<DeliveryOptionsViewModel>("DeliveryOptionsViewModel");

            var itemToRemove = deliveryOptionsViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.FirstOrDefault(x => x.LocationId == locationid);

            if (itemToRemove != null)
            {
                deliveryOptionsViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.Remove(itemToRemove);
            }

            deliveryOptionsViewModel.BlockRelease = false;
            deliveryOptionsViewModel.DayRelease = false;
            deliveryOptionsViewModel.LocationId = null;
            deliveryOptionsViewModel.National = false;
            deliveryOptionsViewModel.Mode = Mode;
            locationid = "";
            _session.SetObject("DeliveryOptionsViewModel", deliveryOptionsViewModel);

            return RedirectToAction("DeliveryOptions", "Apprenticeships");
        }

        [Authorize]
        public IActionResult DeleteCombined(string locationid, ApprenticeshipMode Mode)
        {
            var deliveryOptionsCombinedViewModel = _session.GetObject<DeliveryOptionsCombinedViewModel>("DeliveryOptionsCombinedViewModel");

            var itemToRemove = deliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.FirstOrDefault(x => x.LocationId == locationid);

            if (itemToRemove != null)
            {
                deliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.Remove(itemToRemove);
            }

            deliveryOptionsCombinedViewModel.BlockRelease = false;
            deliveryOptionsCombinedViewModel.DayRelease = false;
            deliveryOptionsCombinedViewModel.LocationId = null;
            deliveryOptionsCombinedViewModel.National = false;

            deliveryOptionsCombinedViewModel.Mode = Mode;
            locationid = "";
            _session.SetObject("DeliveryOptionsCombinedViewModel", deliveryOptionsCombinedViewModel);

            // return View("../ApprenticeshipDeliveryOptions/Index", apprenticeshipDeliveryOptionsViewModel);
            return RedirectToAction("DeliveryOptionsCombined", "Apprenticeships");
        }

        [HttpPost]
        public ActionResult Add(DeliveryOptionsViewModel model)
        {
            var DeliveryOptionsViewModel = _session.GetObject<DeliveryOptionsViewModel>("DeliveryOptionsViewModel");

            if (DeliveryOptionsViewModel == null)
            {
                DeliveryOptionsViewModel = model;
            }

            if (DeliveryOptionsViewModel.DeliveryOptionsListItemModel == null)
            {

                DeliveryOptionsViewModel.DeliveryOptionsListItemModel = new DeliveryOptionsListModel();
                List<DeliveryOptionsListItemModel> list = new List<DeliveryOptionsListItemModel>();
                DeliveryOptionsViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel = list;
            }

            var venue = _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(model.LocationId.Value.ToString()));

            string deliveryMethod = string.Empty;

            if (model.BlockRelease && model.DayRelease)
            {
                deliveryMethod = "Day release, Block release";
            }
            else
            {
                deliveryMethod = model.DayRelease ? "Day release" : "Block release";
            }

            DeliveryOptionsViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.Add(new DeliveryOptionsListItemModel()
            {
                Delivery = deliveryMethod,
                LocationId = venue.Result.Value.ID.ToString(),
                LocationName = venue.Result.Value.VenueName,
                PostCode = venue.Result.Value.PostCode

            });

            DeliveryOptionsViewModel.Mode = model.Mode;

            _session.SetObject("DeliveryOptionsViewModel", DeliveryOptionsViewModel);

            return RedirectToAction("DeliveryOptions", "Apprenticeships", new{Mode = model.Mode});
        }

        public IActionResult Continue(string LocationId, bool DayRelease, bool BlockRelease, int RowCount, ApprenticeshipMode Mode)
        {
            //if (RowCount >= 1)
            //{

            //}
            //else
            //{
            var DeliveryOptionsViewModel = _session.GetObject<DeliveryOptionsViewModel>("DeliveryOptionsViewModel");

            if (DeliveryOptionsViewModel == null)
            {
                DeliveryOptionsViewModel = new DeliveryOptionsViewModel();
                DeliveryOptionsViewModel.Mode = Mode;
            }

            if (DeliveryOptionsViewModel.DeliveryOptionsListItemModel == null)
            {

                DeliveryOptionsViewModel.DeliveryOptionsListItemModel = new DeliveryOptionsListModel();
                List<DeliveryOptionsListItemModel> list = new List<DeliveryOptionsListItemModel>();
                DeliveryOptionsViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel = list;
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

            DeliveryOptionsViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.Add(new DeliveryOptionsListItemModel()
            {
                Delivery = deliveryMethod,
                LocationId = venue.Result.Value.ID.ToString(),
                LocationName = venue.Result.Value.VenueName,
                PostCode = venue.Result.Value.PostCode,
                Radius = null

            });

            _session.SetObject("DeliveryOptionsViewModel", DeliveryOptionsViewModel);


            return Json(Url.Action("Summary", "Apprenticeships", new { Mode = Mode }));
        }


        public IActionResult ContinueCombined(string LocationId, bool DayRelease, bool BlockRelease, bool National, string Radius, int RowCount, ApprenticeshipMode Mode)
        {
            var RadiusValue = 0;

            RadiusValue = National ? 200 : Convert.ToInt32(Radius);

            //if (RowCount >= 1)
            //{

            //}
            //else
            //{
            var DeliveryOptionsCombinedViewModel = _session.GetObject<DeliveryOptionsCombinedViewModel>("DeliveryOptionsCombinedViewModel") ??
                                                                 new DeliveryOptionsCombinedViewModel();

            if (DeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel == null)
            {
                DeliveryOptionsCombinedViewModel.Mode = Mode;
                DeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel = new DeliveryOptionsListModel();
                List<DeliveryOptionsListItemModel> list = new List<DeliveryOptionsListItemModel>();
                DeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel = list;
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

            DeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.Add(new DeliveryOptionsListItemModel()
            {
                Delivery = deliveryMethod,
                LocationId = venue.Result.Value.ID.ToString(),
                LocationName = venue.Result.Value.VenueName,
                PostCode = venue.Result.Value.PostCode,
                National = National,
                Radius = RadiusValue.ToString()

            });

            if (Mode != ApprenticeshipMode.Undefined)
            {
                DeliveryOptionsCombinedViewModel.Mode = Mode;
            }
            _session.SetObject("DeliveryOptionsCombinedViewModel", DeliveryOptionsCombinedViewModel);
            //  }
            return Json(Url.Action("Summary", "Apprenticeships", new { Mode = Mode }));
        }


        [Authorize]
        public IActionResult Complete()
        {
            var model = new CompleteViewModel();

            var DetailViewModel = _session.GetObject<DetailViewModel>("DetailViewModel");

            model.ApprenticeshipName = DetailViewModel.ApprenticeshipTitle;

            _session.Remove("DetailViewModel");
            _session.Remove("DeliveryViewModel");
            _session.Remove("LocationChoiceSelectionViewModel");
            _session.Remove("DeliveryOptionsViewModel");
            _session.Remove("DeliveryOptionsCombinedViewModel");
            _session.Remove("RegionsViewModel");

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
                    return RedirectToAction("Index", "Home");
                case ApprenticeshipWhatWouldYouLikeToDo.View:
                    return RedirectToAction("Index", "ProviderApprenticeships");
                default:
                    return RedirectToAction("Index", "Home");
            }




        }

        [Authorize]
        public IActionResult DeleteConfirm()
        {
            var model = new DeleteConfirmViewModel();


            return View("../Apprenticeships/ConfirmationDelete/Index", model);
        }

        //[Authorize]
        //[HttpPost]
        //public async Task<IActionResult> DeleteConfirm(DeleteConfirmViewModel theModel)
        //{


        //    return RedirectToAction("ConfirmationDelete", "Apprenticeships");

        //}


        [Authorize]
        public IActionResult ConfirmationDelete()
        {
            var model = new ConfirmationDeleteViewModel();

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
            var DeliveryOptionsCombinedViewModel = _session.GetObject<DeliveryOptionsCombinedViewModel>("DeliveryOptionsCombinedViewModel");

            if (DeliveryOptionsCombinedViewModel != null)
            {
                if (DeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel != null)
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
                var DeliveryOptionsCombinedViewModel = _session.GetObject<DeliveryOptionsCombinedViewModel>("DeliveryOptionsCombinedViewModel");

                var item = DeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel
                    .SingleOrDefault(x => x.LocationName == model.LocationName);
                DeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.Remove(item);

                _session.SetObject("DeliveryOptionsCombinedViewModel", DeliveryOptionsCombinedViewModel);
            }
            else
            {
                var DeliveryOptionsViewModel = _session.GetObject<DeliveryOptionsViewModel>("DeliveryOptionsViewModel");

                var item = DeliveryOptionsViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel
                    .SingleOrDefault(x => x.LocationName == model.LocationName);
                DeliveryOptionsViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.Remove(item);

                _session.SetObject("DeliveryOptionsViewModel", DeliveryOptionsViewModel);
            }


            return RedirectToAction(model.Combined ? "DeliveryOptionsCombined" : "DeliveryOptions", "Apprenticeships", new { message = "Location " + model.LocationName + " deleted", mode = model.Mode });
        }



    }
}