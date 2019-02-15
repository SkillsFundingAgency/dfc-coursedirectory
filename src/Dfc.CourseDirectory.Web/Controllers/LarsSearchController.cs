using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class LarsSearchController : Controller
    {
        private readonly ILogger<LarsSearchController> _logger;
        private readonly ILarsSearchSettings _larsSearchSettings;
        private readonly ILarsSearchService _larsSearchService;
        private readonly ILarsSearchHelper _larsSearchHelper;
        private readonly IPaginationHelper _paginationHelper;

        public LarsSearchController(
            ILogger<LarsSearchController> logger,
            IOptions<LarsSearchSettings> larsSearchSettings,
            ILarsSearchService larsSearchService,
            ILarsSearchHelper larsSearchHelper,
            IPaginationHelper paginationHelper)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(larsSearchSettings, nameof(larsSearchSettings));
            Throw.IfNull(larsSearchService, nameof(larsSearchService));
            Throw.IfNull(larsSearchHelper, nameof(larsSearchHelper));
            Throw.IfNull(paginationHelper, nameof(paginationHelper));

            _logger = logger;
            _larsSearchSettings = larsSearchSettings.Value;
            _larsSearchService = larsSearchService;
            _larsSearchHelper = larsSearchHelper;
            _paginationHelper = paginationHelper;
        }
        [Authorize(Policy = "ElevatedUserRole")]
        public async Task<IActionResult> Index([FromQuery] LarsSearchRequestModel requestModel)
        {
            LarsSearchResultModel model;

            if (requestModel == null)
            {
                model = new LarsSearchResultModel();
            }
            else
            {
                var criteria = _larsSearchHelper.GetLarsSearchCriteria(
                    requestModel,
                    _paginationHelper.GetCurrentPageNo(Request.GetDisplayUrl(), _larsSearchSettings.PageParamName),
                    _larsSearchSettings.ItemsPerPage,
                    (LarsSearchFacet[])Enum.GetValues(typeof(LarsSearchFacet)));

                var result = await _larsSearchService.SearchAsync(criteria);

                if (result.IsSuccess && result.HasValue)
                {
                    var filters = _larsSearchHelper.GetLarsSearchFilterModels(result.Value.SearchFacets, requestModel);
                    var items = _larsSearchHelper.GetLarsSearchResultItemModels(result.Value.Value);

                    model = new LarsSearchResultModel(
                        requestModel.SearchTerm,
                        items,
                        Request.GetDisplayUrl(),
                        _larsSearchSettings.PageParamName,
                        _larsSearchSettings.ItemsPerPage,
                        result.Value.ODataCount ?? 0,
                        filters);
                }
                else
                {
                    model = new LarsSearchResultModel(result.Error);
                }
            }
            _logger.LogMethodExit();
            return ViewComponent(nameof(ViewComponents.LarsSearchResult.LarsSearchResult), model);
        }
    }
}