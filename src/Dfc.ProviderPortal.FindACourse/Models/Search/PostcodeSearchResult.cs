
using System;
using System.Collections.Generic;
using Dfc.ProviderPortal.FindACourse.Interfaces;


namespace Dfc.ProviderPortal.FindACourse.Models
{
    public class PostcodeSearchResult : IPostcodeSearchResult
    {
        public string ODataContext { get; set; }
        public int? ODataCount { get; set; }
        public dynamic SearchFacets { get; set; } //PostcodeSearchFacets SearchFacets { get; set; }
        public IEnumerable<dynamic> Value { get; set; } // PostcodeSearchResultItem> Value { get; set; }
}
}
