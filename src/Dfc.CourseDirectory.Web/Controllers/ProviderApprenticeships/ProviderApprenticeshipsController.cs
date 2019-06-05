using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
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
using Dfc.CourseDirectory.Web.ViewModels.ProviderApprenticeships;
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
        private readonly IApprenticeshipService _apprenticeshipService;
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;


        public ProviderApprenticeshipsController(
            ILogger<ProviderApprenticeshipsController> logger,
            IHttpContextAccessor contextAccessor, ICourseService courseService, IVenueService venueService,
            IApprenticeshipService apprenticeshipService)
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
        public async Task<IActionResult> Index()
        {
            int? UKPRN = _session.GetInt32("UKPRN");

            if (!UKPRN.HasValue)
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }
            var result = await _apprenticeshipService.GetApprenticeshipByUKPRN(UKPRN.ToString());
            ProviderApprenticeshipsViewModel model = new ProviderApprenticeshipsViewModel();
            if (result.IsSuccess && result.HasValue)
            {
                model.Apprenticeships = new List<IApprenticeship>();
                foreach(var apprenticeship in result.Value)
                {
                    model.Apprenticeships.Add(apprenticeship);
                }
            }


            return View(model);
        }


        [Authorize]
        public async Task<IActionResult> ProviderApprenticeshipsSearch([FromQuery] ProviderApprenticeShipSearchRequestModel requestModel)
        {

            ProviderApprenticeshipsSearchResultModel model = new ProviderApprenticeshipsSearchResultModel();

            //if (!string.IsNullOrEmpty(requestModel.SearchTerm))
            //{
            //    //stub
            //    var listOfApprenticeships = new List<ProviderApprenticeShipsSearchResultItemModel>();
            //    listOfApprenticeships.Add(new ProviderApprenticeShipsSearchResultItemModel()
            //    {
            //        ApprenticeshipTitle = "Test Apprenticeship 1",
            //        ApprenticeshipType = "Framework",
            //        NotionalNVQLevelv2 = "1 (equivalent to A levels at grades A to E)"
            //    });
            //    listOfApprenticeships.Add(new ProviderApprenticeShipsSearchResultItemModel()
            //    {
            //        ApprenticeshipTitle = "Test Apprenticeship 2",
            //        ApprenticeshipType = string.Empty,
            //        NotionalNVQLevelv2 = "2 (equivalent to A levels at grades A to E)"
            //    });
            //    listOfApprenticeships.Add(new ProviderApprenticeShipsSearchResultItemModel()
            //    {
            //        ApprenticeshipTitle = "Test Apprenticeship 3",
            //        ApprenticeshipType = "Framework",
            //        NotionalNVQLevelv2 = "3 (equivalent to A levels at grades A to E)"
            //    });

            //    model.Items = listOfApprenticeships;
            //}

            return ViewComponent(nameof(ViewComponents.ProviderApprenticeships.ProviderApprenticeshipSearchResult.ProviderApprenticeshipSearchResult), model);
        }

        [Authorize]
        public async Task<IActionResult> DeleteApprenticeship()
        {
            return RedirectToAction("Index", "Apprenticeships");
        }
            




    }
}