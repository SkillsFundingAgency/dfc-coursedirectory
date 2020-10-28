using System.Collections.Generic;

namespace Dfc.ProviderPortal.FindACourse.ApiModels
{
    public class LARSSearchResponse
    {
        public string ODataContext { get; set; }
        public int? ODataCount { get; set; }
        public dynamic SearchFacets { get; set; } //ProviderSearchFacets SearchFacets { get; set; }
        public IEnumerable<dynamic> Value { get; set; } // IEnumerable<LARSSearchResultItem> Value { get; set; }
    }
}
