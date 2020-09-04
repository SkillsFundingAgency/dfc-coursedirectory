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
using Dfc.CourseDirectory.Web.ViewComponents.ZCodeSearchResult;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class ZCodeSearchController : Controller
    {


        public ZCodeSearchController()
        {
          
        }
        [Authorize]
        public IActionResult Index([FromQuery] ZCodeSearchRequestModel requestModel)
        {
            ZCodeSearchResultModel model = new ZCodeSearchResultModel();

            if (requestModel == null)
            {
                //model = new ZCodeSearchResultModel();
            }
            //else
            //{
            //    var criteria = _larsSearchHelper.GetLarsSearchCriteria(
            //        requestModel,
            //        _paginationHelper.GetCurrentPageNo(Request.GetDisplayUrl(), _larsSearchSettings.PageParamName),
            //        _larsSearchSettings.ItemsPerPage,
            //        (LarsSearchFacet[])Enum.GetValues(typeof(LarsSearchFacet)));

            //    var result = await _larsSearchService.SearchAsync(criteria);

            //    if (result.IsSuccess && result.HasValue)
            //    {
            //        var filters = _larsSearchHelper.GetLarsSearchFilterModels(result.Value.SearchFacets, requestModel);
            //        var items = _larsSearchHelper.GetLarsSearchResultItemModels(result.Value.Value);

            //        model = new LarsSearchResultModel(
            //            requestModel.SearchTerm,
            //            items,
            //            Request.GetDisplayUrl(),
            //            _larsSearchSettings.PageParamName,
            //            _larsSearchSettings.ItemsPerPage,
            //            result.Value.ODataCount ?? 0,
            //            filters);
            //    }
            //    else
            //    {
            //        model = new LarsSearchResultModel(result.Error);
            //    }
            //}
            //_logger.LogMethodExit();
            //return ViewComponent(nameof(ViewComponents.LarsSearchResult.LarsSearchResult), model);

            return ViewComponent(nameof(ViewComponents.ZCodeSearchResult.ZCodeSearchResult), model);
        }
    }
}