
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult;


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
        [Authorize]
        public async Task<IActionResult> Index([FromQuery] LarsSearchRequestModel requestModel)
        {
            LarsSearchResultModel model;

            if (requestModel == null) {
                model = new LarsSearchResultModel();

            } else {
                LarsSearchRequestModel requestModelAll = new LarsSearchRequestModel() { SearchTerm = requestModel.SearchTerm };
                var criteriaAll = _larsSearchHelper.GetLarsSearchCriteria(
                    requestModelAll, 1, 
                    _larsSearchSettings.ItemsPerPage,
                    (LarsSearchFacet[])Enum.GetValues(typeof(LarsSearchFacet)));
                var resultAll = await _larsSearchService.SearchAsync(criteriaAll);

                var criteria = _larsSearchHelper.GetLarsSearchCriteria(
                    requestModel,
                    _paginationHelper.GetCurrentPageNo(Request.GetDisplayUrl(), _larsSearchSettings.PageParamName),
                    _larsSearchSettings.ItemsPerPage,
                    (LarsSearchFacet[])Enum.GetValues(typeof(LarsSearchFacet)));

                var result = await _larsSearchService.SearchAsync(criteria);
                if (resultAll.IsSuccess && resultAll.HasValue && result.IsSuccess && result.HasValue)
                {
                    requestModelAll.NotionalNVQLevelv2Filter = requestModel.NotionalNVQLevelv2Filter;
                    requestModelAll.AwardOrgCodeFilter = requestModel.AwardOrgCodeFilter;

                    var filters = _larsSearchHelper.GetLarsSearchFilterModels(resultAll.Value.SearchFacets, requestModelAll);
                    var items = _larsSearchHelper.GetLarsSearchResultItemModels(result.Value.Value);

                    var sfSearch = result.Value.SearchFacets;
                    LarsSearchFilterModel filter = filters.FirstOrDefault(f => f.Title == "Awarding organisation");
                    foreach (SearchFacet sf in resultAll?.Value?.SearchFacets?.AwardOrgCode)
                    {
                        LarsSearchFilterItemModel fim = filter?.Items
                                                              ?.FirstOrDefault(m => m.Value == sf.Value);
                        if (fim != null) //&& filter != null)
                            fim.Count = sfSearch.AwardOrgCode
                                                .FirstOrDefault(f => f.Value == sf.Value)
                                               ?.Count ?? 0;
                    }

                    filter = filters.FirstOrDefault(f => f.Title == "Qualification level");
                    foreach (SearchFacet sf in resultAll?.Value?.SearchFacets?.NotionalNVQLevelv2)
                    {
                        LarsSearchFilterItemModel fim = filter?.Items
                                                              ?.FirstOrDefault(m => m.Value == sf.Value);
                        if (fim != null) //&& filter != null)
                            fim.Count = sfSearch.NotionalNVQLevelv2
                                                .FirstOrDefault(f => f.Value == sf.Value)
                                               ?.Count ?? 0;
                    }

                    model = new LarsSearchResultModel(
                        requestModel.SearchTerm,
                        items,
                        Request.GetDisplayUrl(),
                        _larsSearchSettings.PageParamName,
                        requestModel.PageNo,
                        _larsSearchSettings.ItemsPerPage,
                        result.Value.ODataCount ?? 0,
                        filters);

                } else {
                    model = new LarsSearchResultModel(result.Error);
                }
            }
            _logger.LogMethodExit();
            return ViewComponent(nameof(ViewComponents.LarsSearchResult.LarsSearchResult), model);
        }
    }
}