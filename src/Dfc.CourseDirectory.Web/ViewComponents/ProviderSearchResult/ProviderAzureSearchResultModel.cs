
using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;


namespace Dfc.CourseDirectory.Web.ViewComponents.ProviderSearchResult
{
    public class ProviderAzureSearchResultModel //: IProviderAzureSearchResultModel
    {
        public string ODataContext { get; set; }
        public int? ODataCount { get; set; }
        public dynamic SearchFacets { get; set; } //ProviderSearchFacets SearchFacets { get; set; }
        public IEnumerable<ProviderAzureSearchResultItem> Value { get; set; }
    }
}
