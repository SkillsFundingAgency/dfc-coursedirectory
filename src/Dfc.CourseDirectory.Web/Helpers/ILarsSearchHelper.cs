using System.Collections.Generic;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface ILarsSearchHelper
    {
        ILarsSearchCriteria GetLarsSearchCriteria(LarsSearchRequestModel larsSearchRequestModel, int currentPageNo, int itemsPerPage, IEnumerable<LarsSearchFacet> facets = null);
        IEnumerable<LarsSearchFilterModel> GetLarsSearchFilterModels(LarsSearchFacets larsSearchFacets, LarsSearchRequestModel larsSearchRequestModel);
        IEnumerable<LarsSearchResultItemModel> GetLarsSearchResultItemModels(IEnumerable<LarsSearchResultItem> larsSearchResultItems);
    }
}