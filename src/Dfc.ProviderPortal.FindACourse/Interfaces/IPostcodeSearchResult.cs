
using System;
using System.Collections.Generic;
using Dfc.ProviderPortal.FindACourse.Models;


namespace Dfc.ProviderPortal.FindACourse.Interfaces
{
    public interface IPostcodeSearchResult
    {
        string ODataContext { get; set; }
        int? ODataCount { get; set; }
        dynamic SearchFacets { get; set; } //PostcodeSearchFacets SearchFacets { get; set; }
        IEnumerable<dynamic> Value { get; set; } // IEnumerable<PostcodeSearchResultItem> Value { get; set; }
    }
}
