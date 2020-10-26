using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.ProviderApprenticeships.ProviderApprenticeshipSearchResult;
using Dfc.CourseDirectory.Web.ViewModels.ProviderApprenticeships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{
    [RestrictApprenticeshipQAStatus(ApprenticeshipQAStatus.Passed)]
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
        public async Task<IActionResult> Index(Guid? apprenticeshipId, string message)
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
                var allLiveProviderApprenticeships =
                    result.Value.Where(x => x.RecordStatus == RecordStatus.Live);

                model.Apprenticeships = new List<IApprenticeship>();
                foreach(var apprenticeship in allLiveProviderApprenticeships)
                {
                    model.Apprenticeships.Add(apprenticeship);
                }
            }

            if (apprenticeshipId.HasValue)
            {
                var linkMessage =
                    $"<a id=\"apprenticeshiplink\" class=\"govuk-link\" href=\"#\" data-apprenticeshipid=\"{apprenticeshipId.Value}\">{message}</a>";
                if (!string.IsNullOrEmpty(message)) ViewBag.Message = linkMessage;
                ViewBag.ApprenticeshipId = apprenticeshipId.Value;
            }

            return View(model);
        }


        [Authorize]
        public async Task<IActionResult> ProviderApprenticeshipsSearch([FromQuery] ProviderApprenticeShipSearchRequestModel requestModel)
        {

            ProviderApprenticeshipsSearchResultModel model = new ProviderApprenticeshipsSearchResultModel();
            
            int? UKPRN = _session.GetInt32("UKPRN");
            var result = await _apprenticeshipService.GetApprenticeshipByUKPRN(UKPRN.ToString());


            if (result.IsSuccess && result.HasValue)
            {
                var liveApprenticeships = result.Value.Where(a => a.RecordStatus == RecordStatus.Live);

                if (string.IsNullOrWhiteSpace(requestModel.SearchTerm))
                {
                    model.Items = liveApprenticeships.ToList();
                }
                else
                {
                    var searchTermWords = requestModel.SearchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    model.Items = liveApprenticeships
                        .Where(r => $"{r.ApprenticeshipTitle} {r.MarketingInformation} {r.NotionalNVQLevelv2}".Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .Any(w => searchTermWords.Any(s => s.Equals(w, StringComparison.OrdinalIgnoreCase)))
                            && r.RecordStatus == RecordStatus.Live).ToList();
                }
            }

            return ViewComponent(nameof(ViewComponents.ProviderApprenticeships.ProviderApprenticeshipSearchResult.ProviderApprenticeshipSearchResult), model);
        }

        [Authorize]
        public IActionResult DeleteApprenticeship()
        {
            return RedirectToAction("Index", "Apprenticeships");
        }

    }
}
