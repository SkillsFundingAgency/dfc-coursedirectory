
using System;
using System.Collections.Generic;
using Dfc.ProviderPortal.FindACourse.Models;


namespace Dfc.ProviderPortal.FindACourse.Interfaces
{
    public interface ILARSSearchResult
    {
        string ODataContext { get; set; }
        int? ODataCount { get; set; }
        dynamic SearchFacets { get; set; } //LARSSearchFacets SearchFacets { get; set; }
        IEnumerable<dynamic> Value { get; set; } // IEnumerable<LARSSearchResultItem> Value { get; set; }
    }
}
