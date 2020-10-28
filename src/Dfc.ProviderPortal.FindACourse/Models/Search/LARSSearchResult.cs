
using System;
using System.Collections.Generic;
using Dfc.ProviderPortal.FindACourse.Interfaces;


namespace Dfc.ProviderPortal.FindACourse.Models
{
    public class LARSSearchResult : ILARSSearchResult
    {
        public string ODataContext { get; set; }
        public int? ODataCount { get; set; }
        public dynamic SearchFacets { get; set; } //ProviderSearchFacets SearchFacets { get; set; }
        public IEnumerable<dynamic> Value { get; set; } // IEnumerable<LARSSearchResultItem> Value { get; set; }
    }
}
