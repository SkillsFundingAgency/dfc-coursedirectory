using System;
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
        LarsSearchFilterModel GetLarsSearchFilterModel(string title, string facetName, Func<string, string> textStrategy, IEnumerable<SearchFacet> searchFacets, IEnumerable<string> selectedValues);
        IEnumerable<LarsSearchResultItemModel> GetLarsSearchResultItemModel(IEnumerable<LarsSearchResultItem> larsSearchResultItems);
    }
}