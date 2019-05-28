
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ICourseServiceSettings _courseServiceSettings;
        private readonly ICourseService _courseService;
        private readonly IProviderSearchHelper _providerSearchHelper;
        private readonly IPaginationHelper _paginationHelper;

        public ProviderAzureSearchController(
            ILogger<ProviderAzureSearchController> logger,
            IOptions<CourseServiceSettings> courseServiceSettings,
            ICourseService courseService,
            IProviderSearchHelper providerSearchHelper,
            IPaginationHelper paginationHelper)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(courseServiceSettings, nameof(courseServiceSettings));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(providerSearchHelper, nameof(providerSearchHelper));
            Throw.IfNull(paginationHelper, nameof(paginationHelper));

            _logger = logger;
            _courseServiceSettings = courseServiceSettings.Value;
            _courseService = courseService;
            _providerSearchHelper = providerSearchHelper;
            _paginationHelper = paginationHelper;
        }

        [Authorize]
        public async Task<IActionResult> Index([FromQuery] ProviderSearchCriteria criteria) //ProviderAzureSearchRequestModel requestModel)
        {
            if (criteria == null || string.IsNullOrWhiteSpace(criteria.Keyword))
                return View(new ProviderSearchViewModel() { Search = criteria.Keyword, Providers = new List<ProviderAzureSearchResultItem>() });
            else {
                //ProviderSearchCriteria criteria = new ProviderSearchCriteria() { Keyword = Keyword }; //, Town = new[] { Keyword } };
                ProviderAzureSearchResults result = (await _courseService.ProviderSearchAsync(criteria)).Value;
                ProviderSearchViewModel model = new ProviderSearchViewModel() { Search = criteria.Keyword, Providers = result.Value };
                //return View(model);
                return ViewComponent(nameof(ViewComponents.ProviderAzureSearchResult.ProviderAzureSearchResult), model);
            }
        }
    }
}
