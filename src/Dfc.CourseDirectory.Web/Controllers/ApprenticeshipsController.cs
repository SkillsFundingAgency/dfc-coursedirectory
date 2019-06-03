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

                switch(request.ApprenticeshipType)
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
            var model = new ApprenticeshipDeliveryOptionsViewModel();

            var ApprenticeshipDeliveryOptionsViewModel = _session.GetObject<ApprenticeshipDeliveryOptionsViewModel>("ApprenticeshipDeliveryOptionsViewModel");
            if (ApprenticeshipDeliveryOptionsViewModel != null)
            {
                model = ApprenticeshipDeliveryOptionsViewModel;
                model.BlockRelease = false;
                model.DayRelease = false;
            }
            else
            {
                model.DeliveryOptionsListItemModel = new DeliveryOptionsListModel();
                model.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel = null;
                model.BlockRelease = false;
                model.DayRelease = false;
            }

            
            return View("../ApprenticeshipDeliveryOptions/Index", model);
        }

        [Authorize]
        public IActionResult ApprenticeshipDeliveryOptionsCombined()
        {
            var model = new ApprenticeshipDeliveryOptionsCombinedViewModel();

            var ApprenticeshipDeliveryOptionsCombinedViewModel = _session.GetObject<ApprenticeshipDeliveryOptionsCombinedViewModel>("ApprenticeshipDeliveryOptionsCombinedViewModel");
            if (ApprenticeshipDeliveryOptionsCombinedViewModel != null)
            {
                model = ApprenticeshipDeliveryOptionsCombinedViewModel;
            }
            else
            {
                model.DeliveryOptionsListItemModel = new DeliveryOptionsListModel();
                model.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel = null; ;
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
            var ApprenticeshipRegionsViewModel = _session.GetObject<ApprenticeshipRegionsViewModel>("ApprenticeshipRegionsViewModel");

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

            if (ApprenticeshipRegionsViewModel != null)
            {
                model.ApprenticeshipRegionsViewModel = ApprenticeshipRegionsViewModel;
            }
            else
            {
                model.ApprenticeshipRegionsViewModel = new ApprenticeshipRegionsViewModel();
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
        public async Task<IActionResult> ApprenticeshipSummary(ApprenticeshipSummaryViewModel model)
        {

            Apprenticeship apprenticeship = new Apprenticeship
            {
                id = Guid.NewGuid()
            };
            //Apprenticeship apprenticeship = new Apprenticeship
            //{
            //    //ApprenticeshipId // For backwards compatibility with Tribal (Where does this come from?)
            //    //TribalProviderId // For backwards compatibility with Tribal (Where does this come from?)#

            //    //ProviderId // Is this from our Provider collection?
            //    ProviderUKPRN = int.Parse(_session.GetString("UKPRN")),

            //    ApprenticeshipType = model.ApprenticeshipDetailViewModel.ApprenticeshipType,
            //    StandardCode = model.ApprenticeshipDetailViewModel.StandardCode,
            //    FrameworkCode = model.ApprenticeshipDetailViewModel.FrameworkCode,
            //    ProgType = model.ApprenticeshipDetailViewModel.ProgType,
            //    MarketingInformation = model.ApprenticeshipDetailViewModel.Information,
            //    Url = model.ApprenticeshipDetailViewModel.Website,
            //    ContactTelephone = model.ApprenticeshipDetailViewModel.Telephone,
            //    ContactEmail = model.ApprenticeshipDetailViewModel.Email,
            //    ContactWebsite = model.ApprenticeshipDetailViewModel.ContactUsIUrl,
            //    CreatedDate = DateTime.Now,
            //    CreatedBy = User.Claims.Where(c => c.Type == "email").Select(c => c.Value).SingleOrDefault(),
            //    RecordStatus = RecordStatus.Live,
            //    PathwayCode = model.ApprenticeshipDetailViewModel.PathwayCode,
            //    Version = model.ApprenticeshipDetailViewModel.Version,
            //    NotionalEndLevel = model.ApprenticeshipDetailViewModel.NotionalEndLevel,

            //    //NEED TO ADD
            //    //ApprenticeshipLocations =

                
            //};




            //switch(apprenticeship.ApprenticeshipType)
            //{
            //    case ApprenticeshipType.StandardCode:
            //    {
            //        apprenticeship.StandardId = model.ApprenticeshipDetailViewModel.Id;
            //        break;
            //    }
            //case ApprenticeshipType.FrameworkCode:
            //    {
            //        apprenticeship.FrameworkId = model.ApprenticeshipDetailViewModel.Id;
            //        break;
            //    }
            //}


            var result = await _apprenticeshipService.AddApprenticeship(apprenticeship);

            if(result.IsSuccess)
            {
                return RedirectToAction("ApprenticeshipComplete", "Apprenticeships");
            }
            else
            {
                //Action needs to be decided if failure
                return RedirectToPage("error");
            }
            //_session.Remove("ApprenticeshipDetailViewModel");
            //_session.Remove("ApprenticeshipDeliveryViewModel");
            //_session.Remove("ApprenticeshipLocationChoiceSelectionViewModel");
            //_session.Remove("ApprenticeshipDeliveryOptionsViewModel");
            //_session.Remove("ApprenticeshipRegionsViewModel");
            

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
                PostCode = venue.Result.Value.PostCode,
                Radius = "125 miles"

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
                    Radius = "125 miles"

                });

                _session.SetObject("ApprenticeshipDeliveryOptionsViewModel", ApprenticeshipDeliveryOptionsViewModel);

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