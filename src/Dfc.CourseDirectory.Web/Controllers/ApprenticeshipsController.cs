using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
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
                    foreach(var item in result.Value)
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
                            NotionalEndLevel = item.NotionalEndLevel
                        });
                    }
                }
                
                model.Items = listOfApprenticeships;
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
                model.ApprenticeshipTitle = request.ApprenticeshipTitle;
                model.ApprenticeshipMode = request.ApprenticeshipMode;
                model.ApprenticeshipPreviousPage = request.PreviousPage;
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
                    return View("../ApprenticeShips/Index");
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
            }
            else
            {
                model.DeliveryOptionsListItemModel = new DeliveryOptionsListModel();
                model.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel = null; ;
            }


            return View("../ApprenticeshipDeliveryOptions/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult ApprenticeshipDeliveryOptions(ApprenticeshipDeliveryOptionsViewModel model, string submit)
        {
            _session.SetObject("ApprenticeshipDeliveryOptionsViewModel", model);
            if (submit == "continue")
            {
                return RedirectToAction("ApprenticeshipSummary", "Apprenticeships");
            }
            else
            {
                if (model.DeliveryOptionsListItemModel == null)
                {

                    model.DeliveryOptionsListItemModel = new DeliveryOptionsListModel();
                    List<DeliveryOptionsListItemModel> list = new List<DeliveryOptionsListItemModel>();
                    model.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel = list;
                }

                model.DeliveryOptionsListItemModel.DeliveryOptionsListItemModel.Add(new DeliveryOptionsListItemModel()
                {
                    Delivery = "100% employer based",
                    LocationId = Guid.NewGuid().ToString(),
                    LocationName="Birmingham",
                    Radius = "125 miles"
                
                });

                return View("../ApprenticeshipDeliveryOptions/Index", model);

            }



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
        public IActionResult ApprenticeshipSummary(ApprenticeshipSummaryViewModel model)
        {
            //_session.Remove("ApprenticeshipDetailViewModel");
            //_session.Remove("ApprenticeshipDeliveryViewModel");
            //_session.Remove("ApprenticeshipLocationChoiceSelectionViewModel");
            //_session.Remove("ApprenticeshipDeliveryOptionsViewModel");
            //_session.Remove("ApprenticeshipRegionsViewModel");
            return RedirectToAction("ApprenticeshipComplete", "Apprenticeships");

        }

        [Authorize]
        public IActionResult ApprenticeshipRegions()
        {
            var model = new ApprenticeshipRegionsViewModel();

            model.ChooseRegion = new ChooseRegionModel
            {
                Regions = _courseService.GetRegions()
            };

            var ApprenticeshipRegionsViewModel = _session.GetObject<ApprenticeshipRegionsViewModel>("ApprenticeshipRegionsViewModel");
            if (ApprenticeshipRegionsViewModel != null)
            {
                model = ApprenticeshipRegionsViewModel;
            }



            return View("../ApprenticeshipRegions/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult ApprenticeshipRegions(ApprenticeshipRegionsViewModel model)
        {

            return RedirectToAction("ApprenticeshipSummary", "Apprenticeships");


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