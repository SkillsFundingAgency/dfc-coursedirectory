
using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Common.Interfaces;


namespace Dfc.CourseDirectory.Models.Models.Providers
{
    public class ProviderAzureSearchResults //: IProviderAzureSearchResultModel
    {
        public string ODataContext { get; set; }
        public int? ODataCount { get; set; }
        public dynamic SearchFacets { get; set; } //ProviderSearchFacets SearchFacets { get; set; }
        public IEnumerable<ProviderAzureSearchResultItem> Value { get; set; }
    }
}
