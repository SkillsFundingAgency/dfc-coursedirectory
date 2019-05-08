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
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships.ApprenticeshipSearchResult;
using Dfc.CourseDirectory.Web.ViewModels;
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


        public ApprenticeshipsController(
            ILogger<ApprenticeshipsController> logger,
            IHttpContextAccessor contextAccessor, ICourseService courseService, IVenueService venueService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(venueService, nameof(venueService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _venueService = venueService;
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
                //stub
                var listOfApprenticeships = new List<ApprenticeShipsSearchResultItemModel>();
                listOfApprenticeships.Add(new ApprenticeShipsSearchResultItemModel()
                {
                    ApprenticeshipName = "Test Apprenticeship 1",
                    ApprenticeshipType = "Framework",
                    NotionalNVQLevelv2 = "1 (equivalent to A levels at grades A to E)"
                });
                listOfApprenticeships.Add(new ApprenticeShipsSearchResultItemModel()
                {
                    ApprenticeshipName = "Test Apprenticeship 2",
                    ApprenticeshipType = string.Empty,
                    NotionalNVQLevelv2 = "2 (equivalent to A levels at grades A to E)"
                });
                listOfApprenticeships.Add(new ApprenticeShipsSearchResultItemModel()
                {
                    ApprenticeshipName = "Test Apprenticeship 3",
                    ApprenticeshipType = "Framework",
                    NotionalNVQLevelv2 = "3 (equivalent to A levels at grades A to E)"
                });

                model.Items = listOfApprenticeships;
            }

            return ViewComponent(nameof(ViewComponents.Apprenticeships.ApprenticeshipSearchResult.ApprenticeshipSearchResult), model);
        }


        [Authorize]
        public IActionResult AddApprenticeShipDetails()
        {
            //_session.SetString("Option", "Qualifications");
            return View("../AddApprenticeShipDetails/Index");
        }



    }
}