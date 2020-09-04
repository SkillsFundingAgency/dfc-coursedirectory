
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.ProviderAzureSearchResult;
using Dfc.CourseDirectory.Web.ViewModels.ProviderSearch;


namespace Dfc.CourseDirectory.Web.Controllers
{
    // TODO - Provider search is in the course service for now, needs moving!
    public class ProviderAzureSearchController : Controller
    {
        private readonly ILogger<ProviderAzureSearchController> _logger;
        private readonly ICourseService _courseService;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public ProviderAzureSearchController(
            ILogger<ProviderAzureSearchController> logger,
            ICourseService courseService,
            IHttpContextAccessor contextAccessor)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));

            _logger = logger;
            _courseService = courseService;
            _contextAccessor = contextAccessor;
        }

        [Authorize]
        public async Task<IActionResult> Index([FromQuery] ProviderSearchCriteria criteria)
        {
            if (criteria == null || string.IsNullOrWhiteSpace(criteria.Keyword))
                return View(new ProviderSearchViewModel() { Search = criteria.Keyword, Providers = new List<ProviderAzureSearchResultItem>() });
            else {
                ProviderAzureSearchResults result = (await _courseService.ProviderSearchAsync(criteria)).Value;
                ProviderSearchViewModel model = new ProviderSearchViewModel() { Search = criteria.Keyword, Providers = result.Value };
                return ViewComponent(nameof(ViewComponents.ProviderAzureSearchResult.ProviderAzureSearchResult), model);
            }
        }

        [Authorize]
        public IActionResult SelectProvider([FromQuery] string UKPRN)
        {
            if (string.IsNullOrWhiteSpace(UKPRN) || !int.TryParse(UKPRN, out int value))
                return new NoContentResult();
            else {
                _session.SetInt32("UKPRN", value);
                return RedirectToAction("Index", "Home"); //"Home");
            }
        }
    }
}
