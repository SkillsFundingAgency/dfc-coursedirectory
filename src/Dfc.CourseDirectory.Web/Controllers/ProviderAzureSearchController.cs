
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.ProviderSearchResult;


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
        public async Task<IActionResult> Index([FromQuery] IProviderSearchCriteria criteria) //ProviderAzureSearchRequestModel requestModel)
        {
            ProviderAzureSearchResultModel model = new ProviderAzureSearchResultModel();

            var a = await _courseService.ProviderSearchAsync(criteria);

            //if (requestModel != null) {
            if (criteria != null) {

                //var criteria = _providerSearchHelper.GetAzureSearchCriteria(
                //    requestModel,
                //    _paginationHelper.GetCurrentPageNo(Request.GetDisplayUrl(), _providerSearchSettings.PageParamName),
                //    _providerSearchSettings.ItemsPerPage,
                //    (ProviderSearchFacet[])Enum.GetValues(typeof(ProviderSearchFacet)));

                var result = await _courseService.ProviderSearchAsync(criteria);

                if (result.IsSuccess && result.HasValue) {

                    //var filters = _providerSearchHelper.GetProviderSearchFilterModels(result.Value.SearchFacets, requestModel);
                    //var items = _providerSearchHelper.GetProviderSearchResultItemModels(result.Value.Value);

                    model = new ProviderAzureSearchResultModel(); // (criteria.Keyword, result.Value.Value.Select(p => new Provider() ));
                        //requestModel.SearchTerm,
                        //items,
                        //Request.GetDisplayUrl(),
                        //_providerSearchSettings.PageParamName,
                        //_providerSearchSettings.ItemsPerPage,
                        //result.Value.ODataCount ?? 0,
                        //filters);

                } else {
                    model = new ProviderAzureSearchResultModel(); //ProviderSearchResultModel(result.Error);
                }
            }
            _logger.LogMethodExit();
            return ViewComponent(nameof(ViewComponents.ProviderSearchResult.ProviderSearchResult), model);
        }
    }
}
