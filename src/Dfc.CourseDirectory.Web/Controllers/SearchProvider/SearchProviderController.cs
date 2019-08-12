
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Helpers;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Regions;
using Dfc.CourseDirectory.Models.Models.Venues;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Dfc.CourseDirectory.Web.ViewModels.ProviderSearch;
using Dfc.CourseDirectory.Web.Extensions;
using Dfc.CourseDirectory.Models.Models.Providers;

// There's already a SearchProvider, so this is called SearchProvider to allow route to resolve 
namespace Dfc.CourseDirectory.Web.Controllers.SearchProvider
{
    // TODO - Provider search is in the course service for now, needs moving!
    public class SearchProviderController : Controller
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<ProviderSearchController> _logger;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly ICourseService _courseService;

        public SearchProviderController(
            ILogger<ProviderSearchController> logger,
            IHttpContextAccessor contextAccessor,
            ICourseService courseService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
        }

        [Authorize]
        public async Task<IActionResult> Index(string Keyword)
        {
            ViewBag.HideHeaderBackLink = true;
            if (string.IsNullOrWhiteSpace(Keyword))
                return View(new ProviderSearchViewModel() { Search = Keyword, Providers = new List<ProviderAzureSearchResultItem>() });
            else {
                ProviderSearchCriteria criteria = new ProviderSearchCriteria() { Keyword = Keyword }; //, Town = new[] { Keyword } };
                ProviderAzureSearchResults result = (await _courseService.ProviderSearchAsync(criteria)).Value;
                ProviderSearchViewModel model = new ProviderSearchViewModel() { Search = Keyword, Providers = result.Value };
                return View(model);
            }
        }

    }
}