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

namespace Dfc.CourseDirectory.Web.Controllers
{
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
            _session.Remove("ApprenticeshipDetailViewModel");
            _session.Remove("ApprenticeshipDeliveryViewModel");
            _session.Remove("ApprenticeshipLocationChoiceSelectionViewModel");
            _session.Remove("ApprenticeshipDeliveryOptionsViewModel");
            _session.Remove("ApprenticeshipRegionsViewModel");
            return View();
        }


        [Authorize]
        public async Task<IActionResult> ApprenticeshipSearch([FromQuery] ApprenticeShipSearchRequestModel requestModel)
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

            return ViewComponent(nameof(ViewComponents.Apprenticeships.ApprenticeshipSearchResult.ApprenticeshipSearchResult), model);
        }


        [Authorize]
        public IActionResult ApprenticeShipDetails(ApprenticeShipDetailsRequestModel request)
        {
            var model = new ApprenticeshipDetailViewModel();

            var ApprenticeshipDetailViewModel = _session.GetObject<ApprenticeshipDetailViewModel>("ApprenticeshipDetailViewModel");
            if (ApprenticeshipDetailViewModel != null)
            {
                model = ApprenticeshipDetailViewModel;
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

            return View("../ApprenticeShipDetails/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult ApprenticeShipDetails(ApprenticeshipDetailViewModel model)
        {
            _session.SetObject("ApprenticeshipDetailViewModel", model);

            return RedirectToAction("ApprenticeShipDelivery", "Apprenticeships", new ApprenticeShipDeliveryRequestModel());
        }


        [Authorize]
        public IActionResult ApprenticeShipDelivery()
        {
            var model = new ApprenticeshipDeliveryViewModel();

            var ApprenticeshipDeliveryViewModel = _session.GetObject<ApprenticeshipDeliveryViewModel>("ApprenticeshipDeliveryViewModel");
            if (ApprenticeshipDeliveryViewModel != null)
            {
                model = ApprenticeshipDeliveryViewModel;
            }


            return View("../ApprenticeShipDelivery/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult ApprenticeShipDelivery(ApprenticeshipDeliveryViewModel model)
        {
            _session.SetObject("ApprenticeshipDeliveryViewModel", model);

            switch (model.ApprenticeshipDelivery)
            {
                case ApprenticeshipDelivery.Both:
                    return RedirectToAction("ApprenticeshipDeliveryOptionsCombined", "Apprenticeships");
                case ApprenticeshipDelivery.EmployersAddress:
                    return RedirectToAction("ApprenticeshipLocationChoiceSelection", "Apprenticeships");

                case ApprenticeshipDelivery.YourLocation:
                    return RedirectToAction("ApprenticeshipDeliveryOptions", "Apprenticeships");
                default:
                    return View("../ApprenticeShips/Index");

            }

        }


        [Authorize]
        public IActionResult ApprenticeshipLocationChoiceSelection()
        {
            var model = new ApprenticeshipLocationChoiceSelectionViewModel();

            var ApprenticeshipLocationChoiceSelectionViewModel = _session.GetObject<ApprenticeshipLocationChoiceSelectionViewModel>("ApprenticeshipLocationChoiceSelectionViewModel");
            if (ApprenticeshipLocationChoiceSelectionViewModel != null)
            {
                model = ApprenticeshipLocationChoiceSelectionViewModel;
            }

            return View("../ApprenticeshipLocationChoiceSelection/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult ApprenticeshipLocationChoiceSelection(ApprenticeshipLocationChoiceSelectionViewModel model)
        {
            _session.SetObject("ApprenticeshipLocationChoiceSelectionViewModel", model);
            switch (model.NationalApprenticeship)
            {
                case NationalApprenticeship.Yes:
                    return RedirectToAction("ApprenticeshipSummary", "Apprenticeships");

                case NationalApprenticeship.No:
                    return RedirectToAction("ApprenticeshipRegions", "Apprenticeships");

                default:
                    return View("../ApprenticeShips/Index");

            }
        }



        [Authorize]
        public IActionResult ApprenticeshipDeliveryOptions()
        {
            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var model = new ApprenticeshipDeliveryOptionsViewModel();

            var ApprenticeshipDeliveryOptionsViewModel = _session.GetObject<ApprenticeshipDeliveryOptionsViewModel>("ApprenticeshipDeliveryOptionsViewModel");
            if (ApprenticeshipDeliveryOptionsViewModel != null)
            {
                model = ApprenticeshipDeliveryOptionsViewModel;
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


            return View("../ApprenticeshipDeliveryOptions/Index", model);
        }

        [Authorize]
        public IActionResult ApprenticeshipDeliveryOptionsCombined()
        {
            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var model = new ApprenticeshipDeliveryOptionsCombinedViewModel();

            var ApprenticeshipDeliveryOptionsCombinedViewModel = _session.GetObject<ApprenticeshipDeliveryOptionsCombinedViewModel>("ApprenticeshipDeliveryOptionsCombinedViewModel");
            if (ApprenticeshipDeliveryOptionsCombinedViewModel != null)
            {
                model = ApprenticeshipDeliveryOptionsCombinedViewModel;
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

            return View("../ApprenticeshipDeliveryOptionsCombined/Index", model);
        }



        [Authorize]
        public IActionResult ApprenticeshipSummary()
        {
            var model = new ApprenticeshipSummaryViewModel();

            var ApprenticeshipDetailViewModel = _session.GetObject<ApprenticeshipDetailViewModel>("ApprenticeshipDetailViewModel");
            var ApprenticeshipDeliveryViewModel = _session.GetObject<ApprenticeshipDeliveryViewModel>("ApprenticeshipDeliveryViewModel");
            var ApprenticeshipLocationChoiceSelectionViewModel = _session.GetObject<ApprenticeshipLocationChoiceSelectionViewModel>("ApprenticeshipLocationChoiceSelectionViewModel");
            var ApprenticeshipDeliveryOptionsViewModel = _session.GetObject<ApprenticeshipDeliveryOptionsViewModel>("ApprenticeshipDeliveryOptionsViewModel");
            var ApprenticeshipDeliveryOptionsCombinedViewModel = _session.GetObject<ApprenticeshipDeliveryOptionsCombinedViewModel>("ApprenticeshipDeliveryOptionsCombinedViewModel");
            var ApprenticeshipRegions = _session.GetObject<String[]>("SelectedRegions");




            if (ApprenticeshipDetailViewModel != null)
            {
                model.ApprenticeshipDetailViewModel = ApprenticeshipDetailViewModel;
            }
            else
            {
                model.ApprenticeshipDetailViewModel = new ApprenticeshipDetailViewModel() { ApprenticeshipTitle = "Test" };
            }

            if (ApprenticeshipDeliveryViewModel != null)
            {
                model.ApprenticeshipDeliveryViewModel = ApprenticeshipDeliveryViewModel;
            }
            else
            {
                model.ApprenticeshipDeliveryViewModel = new ApprenticeshipDeliveryViewModel();
            }

            if (ApprenticeshipDeliveryOptionsViewModel != null)
            {
                model.ApprenticeshipDeliveryOptionsViewModel = ApprenticeshipDeliveryOptionsViewModel;
            }
            else
            {
                model.ApprenticeshipDeliveryOptionsViewModel = new ApprenticeshipDeliveryOptionsViewModel();
            }

            if (ApprenticeshipDeliveryOptionsCombinedViewModel != null)
            {
                model.ApprenticeshipDeliveryOptionsCombinedViewModel = ApprenticeshipDeliveryOptionsCombinedViewModel;
            }
            else
            {
                model.ApprenticeshipDeliveryOptionsCombinedViewModel = new ApprenticeshipDeliveryOptionsCombinedViewModel();
            }

            if (ApprenticeshipRegions != null)
            {
                model.ApprenticeshipRegions = ApprenticeshipRegions;
            }
            else
            {
                model.ApprenticeshipRegions = new string[0];
            }


            if (ApprenticeshipLocationChoiceSelectionViewModel != null)
            {

                model.ApprenticeshipLocationChoiceSelectionViewModel = ApprenticeshipLocationChoiceSelectionViewModel;
            }
            else
            {
                model.ApprenticeshipLocationChoiceSelectionViewModel = new ApprenticeshipLocationChoiceSelectionViewModel();
            }

            return View("../ApprenticeshipSummary/Index", model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ApprenticeshipSummary(ApprenticeshipSummaryViewModel theModel)
        {

            var model = new ApprenticeshipSummaryViewModel();

            var ApprenticeshipDetailViewModel = _session.GetObject<ApprenticeshipDetailViewModel>("ApprenticeshipDetailViewModel");
            var ApprenticeshipDeliveryViewModel = _session.GetObject<ApprenticeshipDeliveryViewModel>("ApprenticeshipDeliveryViewModel");
            var ApprenticeshipLocationChoiceSelectionViewModel = _session.GetObject<ApprenticeshipLocationChoiceSelectionViewModel>("ApprenticeshipLocationChoiceSelectionViewModel");
            var ApprenticeshipDeliveryOptionsViewModel = _session.GetObject<ApprenticeshipDeliveryOptionsViewModel>("ApprenticeshipDeliveryOptionsViewModel");
            var ApprenticeshipDeliveryOptionsCombinedViewModel = _session.GetObject<ApprenticeshipDeliveryOptionsCombinedViewModel>("ApprenticeshipDeliveryOptionsCombinedViewModel");
            var ApprenticeshipRegions = _session.GetObject<String[]>("SelectedRegions");




            if (ApprenticeshipDetailViewModel != null)
            {
                model.ApprenticeshipDetailViewModel = ApprenticeshipDetailViewModel;
            }
            else
            {
                model.ApprenticeshipDetailViewModel = new ApprenticeshipDetailViewModel() { ApprenticeshipTitle = "Test" };
            }

            if (ApprenticeshipDeliveryViewModel != null)
            {
                model.ApprenticeshipDeliveryViewModel = ApprenticeshipDeliveryViewModel;
            }
            else
            {
                model.ApprenticeshipDeliveryViewModel = new ApprenticeshipDeliveryViewModel();
            }

            if (ApprenticeshipDeliveryOptionsViewModel != null)
            {
                model.ApprenticeshipDeliveryOptionsViewModel = ApprenticeshipDeliveryOptionsViewModel;
            }
            else
            {
                model.ApprenticeshipDeliveryOptionsViewModel = new ApprenticeshipDeliveryOptionsViewModel();
            }

            if (ApprenticeshipDeliveryOptionsCombinedViewModel != null)
            {
                model.ApprenticeshipDeliveryOptionsCombinedViewModel = ApprenticeshipDeliveryOptionsCombinedViewModel;
            }
            else
            {
                model.ApprenticeshipDeliveryOptionsCombinedViewModel = new ApprenticeshipDeliveryOptionsCombinedViewModel();
            }

            if (ApprenticeshipRegions != null)
            {
                model.ApprenticeshipRegions = ApprenticeshipRegions;
            }
            else
            {
                model.ApprenticeshipRegions = new string[0];
            }

            if (ApprenticeshipLocationChoiceSelectionViewModel != null)
            {

                model.ApprenticeshipLocationChoiceSelectionViewModel = ApprenticeshipLocationChoiceSelectionViewModel;
            }
            else
            {
                model.ApprenticeshipLocationChoiceSelectionViewModel = new ApprenticeshipLocationChoiceSelectionViewModel();
            }

            ApprenticeshipLocationType apprenticeshipLocationType = new ApprenticeshipLocationType();

            if (model.ApprenticeshipDeliveryViewModel.ApprenticeshipDelivery == ApprenticeshipDelivery.YourLocation)
            {
                apprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBased;
            }

            if (model.ApprenticeshipDeliveryViewModel.ApprenticeshipDelivery == ApprenticeshipDelivery.EmployersAddress)
            {
                apprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased;
            }

            if (model.ApprenticeshipDeliveryViewModel.ApprenticeshipDelivery == ApprenticeshipDelivery.Both)
            {
                apprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBasedAndEmployerBased;
            }



            int UKPRN;

            if (_session.GetInt32("UKPRN") != null)
            {
                UKPRN = _session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }



            List<ApprenticeshipLocation> locations = new List<ApprenticeshipLocation>();

            foreach (var loc in model.ApprenticeshipDeliveryOptionsViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel)
            {
                List<int> deliveryModes = new List<int>();

                ApprenticeshipLocation apprenticeshipLocation = new ApprenticeshipLocation()
                {
                    CreatedDate = DateTime.Now,
                    CreatedBy = User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault(),
                    ApprenticeshipLocationType = apprenticeshipLocationType,
                    id = Guid.NewGuid(),
                    LocationType = LocationType.Region,
                    RecordStatus = RecordStatus.Live,
                    
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
                    if (delMode.ToLower() == @WebHelper.GetEnumDescription(ApprenticeShipDeliveryLocation.DayRelease).ToLower())
                    {
                        deliveryModes.Add((int)ApprenticeShipDeliveryLocation.DayRelease);
                    }

                    if (delMode.ToLower() == @WebHelper.GetEnumDescription(ApprenticeShipDeliveryLocation.BlockRelease).ToLower())
                    {
                        deliveryModes.Add((int)ApprenticeShipDeliveryLocation.BlockRelease);
                    }
                }

                apprenticeshipLocation.DeliveryModes = deliveryModes;

                locations.Add(apprenticeshipLocation);
            }

            Apprenticeship apprenticeship = new Apprenticeship();


            //ApprenticeshipId // For backwards compatibility with Tribal (Where does this come from?)
            //TribalProviderId // For backwards compatibility with Tribal (Where does this come from?)#

            //ProviderId // Is this from our Provider collection?

            apprenticeship.id = Guid.NewGuid();
            apprenticeship.ProviderUKPRN = UKPRN;

            apprenticeship.ApprenticeshipType = model.ApprenticeshipDetailViewModel.ApprenticeshipType;
            apprenticeship.StandardCode = model.ApprenticeshipDetailViewModel.StandardCode;
            apprenticeship.FrameworkCode = model.ApprenticeshipDetailViewModel.FrameworkCode;
            apprenticeship.ProgType = model.ApprenticeshipDetailViewModel.ProgType;
            apprenticeship.MarketingInformation = model.ApprenticeshipDetailViewModel.Information;
            apprenticeship.Url = model.ApprenticeshipDetailViewModel.Website;
            apprenticeship.ContactTelephone = model.ApprenticeshipDetailViewModel.Telephone;
            apprenticeship.ContactEmail = model.ApprenticeshipDetailViewModel.Email;
            apprenticeship.ContactWebsite = model.ApprenticeshipDetailViewModel.ContactUsIUrl;
            apprenticeship.CreatedDate = DateTime.Now;
            apprenticeship.CreatedBy = User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault();
            apprenticeship.RecordStatus = RecordStatus.Live;
            apprenticeship.PathwayCode = model.ApprenticeshipDetailViewModel.PathwayCode;
            apprenticeship.Version = model.ApprenticeshipDetailViewModel.Version;
            apprenticeship.NotionalEndLevel = model.ApprenticeshipDetailViewModel.NotionalEndLevel;

            //NEED TO ADD
            apprenticeship.ApprenticeshipLocations = locations;






            switch (apprenticeship.ApprenticeshipType)
            {
                case ApprenticeshipType.StandardCode:
                    {
                        apprenticeship.StandardId = model.ApprenticeshipDetailViewModel.Id;
                        break;
                    }
                case ApprenticeshipType.FrameworkCode:
                    {
                        apprenticeship.FrameworkId = model.ApprenticeshipDetailViewModel.Id;
                        break;
                    }
            }


            var result = await _apprenticeshipService.AddApprenticeship(apprenticeship);

            if (result.IsSuccess)
            {
                return RedirectToAction("ApprenticeshipComplete", "Apprenticeships");
            }
            else
            {
                //Action needs to be decided if failure
                return RedirectToAction("ApprenticeshipCSummary", "Apprenticeships");
            }



        }

        [Authorize]
        public IActionResult ApprenticeshipRegions()
        {
            var model = new ApprenticeshipRegionsViewModel();

            model.ChooseRegion = new ChooseRegionModel
            {
                Regions = _courseService.GetRegions()
            };

            //var ApprenticeshipRegionsViewModel = _session.GetObject<ApprenticeshipRegionsViewModel>("ApprenticeshipRegionsViewModel");
            //if (ApprenticeshipRegionsViewModel != null)
            //{
            //    model = ApprenticeshipRegionsViewModel;
            //}

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


            return View("../ApprenticeshipRegions/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult ApprenticeshipRegions(string[] SelectedRegions)
        {
            _session.SetObject("SelectedRegions", SelectedRegions);

            return RedirectToAction("ApprenticeshipSummary", "Apprenticeships");


        }

        [HttpPost]
        public ActionResult AddCombined(ApprenticeshipDeliveryOptionsCombinedViewModel model)
        {
            var ApprenticeshipDeliveryOptionsCombinedViewModel = _session.GetObject<ApprenticeshipDeliveryOptionsCombinedViewModel>("ApprenticeshipDeliveryOptionsCombinedViewModel");

            if (ApprenticeshipDeliveryOptionsCombinedViewModel == null)
            {
                ApprenticeshipDeliveryOptionsCombinedViewModel = model;
            }

            if (ApprenticeshipDeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel == null)
            {

                ApprenticeshipDeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel = new DeliveryOptionsListModel();
                List<DeliveryOptionsListItemModel> list = new List<DeliveryOptionsListItemModel>();
                ApprenticeshipDeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel = list;
            }

            var venue = _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(model.LocationId.Value.ToString()));

            string deliveryMethod = string.Empty;

            if (model.BlockRelease && model.DayRelease)
            {
                deliveryMethod = "Day release, Block release";
            }
            else
            {
                if (model.DayRelease)
                {
                    deliveryMethod = "Day release";
                }
                else
                {
                    deliveryMethod = "Block release";
                }
            }

            ApprenticeshipDeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.Add(new DeliveryOptionsListItemModel()
            {
                Delivery = deliveryMethod,
                LocationId = venue.Result.Value.ID.ToString(),
                LocationName = venue.Result.Value.VenueName,
                PostCode = venue.Result.Value.PostCode,
                Radius = model.Radius,
                National = model.National

            });

            _session.SetObject("ApprenticeshipDeliveryOptionsCombinedViewModel", ApprenticeshipDeliveryOptionsCombinedViewModel);

            return RedirectToAction("ApprenticeshipDeliveryOptionsCombined", "Apprenticeships");
        }

        [Authorize]
        public IActionResult Delete(string locationid)
        {
            var apprenticeshipDeliveryOptionsViewModel = _session.GetObject<ApprenticeshipDeliveryOptionsViewModel>("ApprenticeshipDeliveryOptionsViewModel");

            var itemToRemove = apprenticeshipDeliveryOptionsViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.Where(x => x.LocationId == locationid).FirstOrDefault();

            if (itemToRemove != null)
            {
                apprenticeshipDeliveryOptionsViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.Remove(itemToRemove);
            }

            apprenticeshipDeliveryOptionsViewModel.BlockRelease = false;
            apprenticeshipDeliveryOptionsViewModel.DayRelease = false;
            apprenticeshipDeliveryOptionsViewModel.LocationId = null;
            apprenticeshipDeliveryOptionsViewModel.National = false;
            locationid = "";
            _session.SetObject("ApprenticeshipDeliveryOptionsViewModel", apprenticeshipDeliveryOptionsViewModel);

            // return View("../ApprenticeshipDeliveryOptions/Index", apprenticeshipDeliveryOptionsViewModel);
            return RedirectToAction("ApprenticeshipDeliveryOptions", "Apprenticeships");
        }

        [Authorize]
        public IActionResult DeleteCombined(string locationid)
        {
            var apprenticeshipDeliveryOptionsCombinedViewModel = _session.GetObject<ApprenticeshipDeliveryOptionsCombinedViewModel>("ApprenticeshipDeliveryOptionsCombinedViewModel");

            var itemToRemove = apprenticeshipDeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.Where(x => x.LocationId == locationid).FirstOrDefault();

            if (itemToRemove != null)
            {
                apprenticeshipDeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.Remove(itemToRemove);
            }

            apprenticeshipDeliveryOptionsCombinedViewModel.BlockRelease = false;
            apprenticeshipDeliveryOptionsCombinedViewModel.DayRelease = false;
            apprenticeshipDeliveryOptionsCombinedViewModel.LocationId = null;
            apprenticeshipDeliveryOptionsCombinedViewModel.National = false;
            locationid = "";
            _session.SetObject("ApprenticeshipDeliveryOptionsCombinedViewModel", apprenticeshipDeliveryOptionsCombinedViewModel);

            // return View("../ApprenticeshipDeliveryOptions/Index", apprenticeshipDeliveryOptionsViewModel);
            return RedirectToAction("ApprenticeshipDeliveryOptionsCombined", "Apprenticeships");
        }

        [HttpPost]
        public ActionResult Add(ApprenticeshipDeliveryOptionsViewModel model)
        {
            var ApprenticeshipDeliveryOptionsViewModel = _session.GetObject<ApprenticeshipDeliveryOptionsViewModel>("ApprenticeshipDeliveryOptionsViewModel");

            if (ApprenticeshipDeliveryOptionsViewModel == null)
            {
                ApprenticeshipDeliveryOptionsViewModel = model;
            }

            if (ApprenticeshipDeliveryOptionsViewModel.DeliveryOptionsListItemModel == null)
            {

                ApprenticeshipDeliveryOptionsViewModel.DeliveryOptionsListItemModel = new DeliveryOptionsListModel();
                List<DeliveryOptionsListItemModel> list = new List<DeliveryOptionsListItemModel>();
                ApprenticeshipDeliveryOptionsViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel = list;
            }

            var venue = _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(model.LocationId.Value.ToString()));

            string deliveryMethod = string.Empty;

            if (model.BlockRelease && model.DayRelease)
            {
                deliveryMethod = "Day release, Block release";
            }
            else
            {
                if (model.DayRelease)
                {
                    deliveryMethod = "Day release";
                }
                else
                {
                    deliveryMethod = "Block release";
                }
            }

            ApprenticeshipDeliveryOptionsViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.Add(new DeliveryOptionsListItemModel()
            {
                Delivery = deliveryMethod,
                LocationId = venue.Result.Value.ID.ToString(),
                LocationName = venue.Result.Value.VenueName,
                PostCode = venue.Result.Value.PostCode

            });

            _session.SetObject("ApprenticeshipDeliveryOptionsViewModel", ApprenticeshipDeliveryOptionsViewModel);

            return RedirectToAction("ApprenticeshipDeliveryOptions", "Apprenticeships");
        }

        public IActionResult Continue(string LocationId, bool DayRelease, bool BlockRelease, int RowCount)
        {
            if (RowCount >= 1)
            {

            }
            else
            {
                var ApprenticeshipDeliveryOptionsViewModel = _session.GetObject<ApprenticeshipDeliveryOptionsViewModel>("ApprenticeshipDeliveryOptionsViewModel");

                if (ApprenticeshipDeliveryOptionsViewModel == null)
                {
                    ApprenticeshipDeliveryOptionsViewModel = new ApprenticeshipDeliveryOptionsViewModel();
                }

                if (ApprenticeshipDeliveryOptionsViewModel.DeliveryOptionsListItemModel == null)
                {

                    ApprenticeshipDeliveryOptionsViewModel.DeliveryOptionsListItemModel = new DeliveryOptionsListModel();
                    List<DeliveryOptionsListItemModel> list = new List<DeliveryOptionsListItemModel>();
                    ApprenticeshipDeliveryOptionsViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel = list;
                }

                var venue = _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(LocationId));

                string deliveryMethod = string.Empty;

                if (BlockRelease && DayRelease)
                {
                    deliveryMethod = "Day release, Block release";
                }
                else
                {
                    if (DayRelease)
                    {
                        deliveryMethod = "Day release";
                    }
                    else
                    {
                        deliveryMethod = "Block release";
                    }
                }

                ApprenticeshipDeliveryOptionsViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.Add(new DeliveryOptionsListItemModel()
                {
                    Delivery = deliveryMethod,
                    LocationId = venue.Result.Value.ID.ToString(),
                    LocationName = venue.Result.Value.VenueName,
                    PostCode = venue.Result.Value.PostCode,
                    Radius = null

                });

                _session.SetObject("ApprenticeshipDeliveryOptionsViewModel", ApprenticeshipDeliveryOptionsViewModel);

                //return RedirectToAction("ApprenticeshipDeliveryOptions", "Apprenticeships");
            }
            return Json(Url.Action("ApprenticeshipSummary", "Apprenticeships"));
        }


        public IActionResult ContinueCombined(string LocationId, bool DayRelease, bool BlockRelease, bool National, string Radius, int RowCount)
        {
            if (RowCount >= 1)
            {

            }
            else
            {
                var ApprenticeshipDeliveryOptionsCombinedViewModel = _session.GetObject<ApprenticeshipDeliveryOptionsCombinedViewModel>("ApprenticeshipDeliveryOptionsCombinedViewModel");

                if (ApprenticeshipDeliveryOptionsCombinedViewModel == null)
                {
                    ApprenticeshipDeliveryOptionsCombinedViewModel = new ApprenticeshipDeliveryOptionsCombinedViewModel();
                }

                if (ApprenticeshipDeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel == null)
                {

                    ApprenticeshipDeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel = new DeliveryOptionsListModel();
                    List<DeliveryOptionsListItemModel> list = new List<DeliveryOptionsListItemModel>();
                    ApprenticeshipDeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel = list;
                }

                var venue = _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(LocationId));

                string deliveryMethod = string.Empty;

                if (BlockRelease && DayRelease)
                {
                    deliveryMethod = "Day release, Block release";
                }
                else
                {
                    if (DayRelease)
                    {
                        deliveryMethod = "Day release";
                    }
                    else
                    {
                        deliveryMethod = "Block release";
                    }
                }

                ApprenticeshipDeliveryOptionsCombinedViewModel.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.Add(new DeliveryOptionsListItemModel()
                {
                    Delivery = deliveryMethod,
                    LocationId = venue.Result.Value.ID.ToString(),
                    LocationName = venue.Result.Value.VenueName,
                    PostCode = venue.Result.Value.PostCode,
                    National = National,
                    Radius = Radius

                });

                _session.SetObject("ApprenticeshipDeliveryOptionsCombinedViewModel", ApprenticeshipDeliveryOptionsCombinedViewModel);

                //return RedirectToAction("ApprenticeshipDeliveryOptions", "Apprenticeships");
            }
            return Json(Url.Action("ApprenticeshipSummary", "Apprenticeships"));
        }


        [Authorize]
        public IActionResult ApprenticeshipComplete()
        {
            var model = new ApprenticeshipCompleteViewModel();

            var ApprenticeshipDetailViewModel = _session.GetObject<ApprenticeshipDetailViewModel>("ApprenticeshipDetailViewModel");

            model.ApprenticeshipName = ApprenticeshipDetailViewModel.ApprenticeshipTitle;

            _session.Remove("ApprenticeshipDetailViewModel");
            _session.Remove("ApprenticeshipDeliveryViewModel");
            _session.Remove("ApprenticeshipLocationChoiceSelectionViewModel");
            _session.Remove("ApprenticeshipDeliveryOptionsViewModel");
            _session.Remove("ApprenticeshipRegionsViewModel");

            return View("../ApprenticeshipComplete/Index", model);
        }

        [Authorize]
        public IActionResult AddAnotherApprenticeship()
        {



            return RedirectToAction("Index", "Apprenticeships");


        }



    }
}