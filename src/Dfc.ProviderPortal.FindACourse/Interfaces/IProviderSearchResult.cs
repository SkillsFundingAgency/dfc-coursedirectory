
using System;
using System.Collections.Generic;
using Dfc.ProviderPortal.FindACourse.Models;


namespace Dfc.ProviderPortal.FindACourse.Interfaces
{
    public interface IProviderSearchResult
    {
        string ODataContext { get; set; }
        int? ODataCount { get; set; }
        dynamic SearchFacets { get; set; } //ProviderSearchFacets SearchFacets { get; set; }
        IEnumerable<ProviderSearchResultItem> Value { get; set; }
    }
}
