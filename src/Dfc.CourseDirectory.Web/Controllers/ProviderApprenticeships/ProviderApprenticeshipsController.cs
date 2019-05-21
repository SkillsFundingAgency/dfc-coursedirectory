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
using Dfc.CourseDirectory.Web.Extensions;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships.ApprenticeshipSearchResult;
using Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseRegion;
using Dfc.CourseDirectory.Web.ViewComponents.ProviderApprenticeships.ProviderApprenticeshipSearchResult;
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
    public class ProviderApprenticeshipsController : Controller
    {
        private readonly ILogger<ProviderApprenticeshipsController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;


        public ProviderApprenticeshipsController(
            ILogger<ProviderApprenticeshipsController> logger,
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

            return View();
        }


        [Authorize]
        public async Task<IActionResult> ProviderApprenticeshipsSearch([FromQuery] ProviderApprenticeShipSearchRequestModel requestModel)
        {

            ProviderApprenticeshipsSearchResultModel model = new ProviderApprenticeshipsSearchResultModel();

            if (!string.IsNullOrEmpty(requestModel.SearchTerm))
            {
                //stub
                var listOfApprenticeships = new List<ProviderApprenticeShipsSearchResultItemModel>();
                listOfApprenticeships.Add(new ProviderApprenticeShipsSearchResultItemModel()
                {
                    ApprenticeshipTitle = "Test Apprenticeship 1",
                    ApprenticeshipType = "Framework",
                    NotionalNVQLevelv2 = "1 (equivalent to A levels at grades A to E)"
                });
                listOfApprenticeships.Add(new ProviderApprenticeShipsSearchResultItemModel()
                {
                    ApprenticeshipTitle = "Test Apprenticeship 2",
                    ApprenticeshipType = string.Empty,
                    NotionalNVQLevelv2 = "2 (equivalent to A levels at grades A to E)"
                });
                listOfApprenticeships.Add(new ProviderApprenticeShipsSearchResultItemModel()
                {
                    ApprenticeshipTitle = "Test Apprenticeship 3",
                    ApprenticeshipType = "Framework",
                    NotionalNVQLevelv2 = "3 (equivalent to A levels at grades A to E)"
                });

                model.Items = listOfApprenticeships;
            }

            return ViewComponent(nameof(ViewComponents.ProviderApprenticeships.ProviderApprenticeshipSearchResult.ProviderApprenticeshipSearchResult), model);
        }

        [Authorize]
        public async Task<IActionResult> DeleteApprenticeship()
        {
            return RedirectToAction("Index", "Apprenticeships");
        }
            




    }
}