using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships.ApprenticeshipSearchResult;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.Apprenticeships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class ApprenticeshipsController : Controller
    {
        private readonly ILogger<ApprenticeshipsController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;
        //private readonly IApprenticeshipService _apprenticeshipService;

        public ApprenticeshipsController(
            ILogger<ApprenticeshipsController> logger,
            IHttpContextAccessor contextAccessor, ICourseService courseService, IVenueService venueService
            /*,IApprenticeshipService apprenticeshipService*/)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(venueService, nameof(venueService));
            //Throw.IfNull(apprenticeshipService, nameof(apprenticeshipService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _venueService = venueService;
            //_apprenticeshipService = apprenticeshipService;
        }

        [Authorize]
        public IActionResult Index()
        {
            //_session.SetString("Option", "Qualifications");
            return View();
        }


        [Authorize]
        public async Task<IActionResult> ApprenticeshipSearch([FromQuery] ApprenticeShipSearchRequestModel requestModel)
        {
            ApprenticeshipsSearchResultModel model = new ApprenticeshipsSearchResultModel();

            if (!string.IsNullOrEmpty(requestModel.SearchTerm))
            {
               // var result = _apprenticeshipService.StandardsAndFrameworksSearch(requestModel.SearchTerm);

                //stub
                var listOfApprenticeships = new List<ApprenticeShipsSearchResultItemModel>();
                listOfApprenticeships.Add(new ApprenticeShipsSearchResultItemModel()
                {
                    ApprenticeshipTitle = "Test Apprenticeship 1",
                    ApprenticeshipType = "Framework",
                    NotionalNVQLevelv2 = "1 (equivalent to A levels at grades A to E)"
                });
                listOfApprenticeships.Add(new ApprenticeShipsSearchResultItemModel()
                {
                    ApprenticeshipTitle = "Test Apprenticeship 2",
                    ApprenticeshipType = string.Empty,
                    NotionalNVQLevelv2 = "2 (equivalent to A levels at grades A to E)"
                });
                listOfApprenticeships.Add(new ApprenticeShipsSearchResultItemModel()
                {
                    ApprenticeshipTitle = "Test Apprenticeship 3",
                    ApprenticeshipType = "Framework",
                    NotionalNVQLevelv2 = "3 (equivalent to A levels at grades A to E)"
                });

                model.Items = listOfApprenticeships;
            }

            return ViewComponent(nameof(ViewComponents.Apprenticeships.ApprenticeshipSearchResult.ApprenticeshipSearchResult), model);
        }


        [Authorize]
        public IActionResult ApprenticeShipDetails(ApprenticeShipDetailsRequestModel request)
        {
            var model = new ApprenticeshipDetailViewModel();
            model.ApprenticeshipTitle = request.ApprenticeshipTitle;

            return View("../ApprenticeShipDetails/Index",model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult ApprenticeShipDetails(ApprenticeshipDetailViewModel model)
        {
            //_session.SetString("Option", "Qualifications");
            //return View("../ApprenticeShips/Index");

            return RedirectToAction("ApprenticeShipDelivery", "Apprenticeships", new ApprenticeShipDeliveryRequestModel());
        }


        [Authorize]
        public IActionResult ApprenticeShipDelivery(ApprenticeShipDeliveryRequestModel request)
        {
            var model = new ApprenticeshipDeliveryViewModel();

            return View("../ApprenticeShipDelivery/Index", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult ApprenticeShipDelivery(ApprenticeshipDeliveryViewModel model)
        {
            //_session.SetString("Option", "Qualifications");
            return View("../ApprenticeShips/Index");
        }



    }
}