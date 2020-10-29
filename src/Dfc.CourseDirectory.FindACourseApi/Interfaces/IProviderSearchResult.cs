
using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.FindACourseApi.Models;


namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface IProviderSearchResult
    {
        string ODataContext { get; set; }
        int? ODataCount { get; set; }
        dynamic SearchFacets { get; set; } //ProviderSearchFacets SearchFacets { get; set; }
        IEnumerable<ProviderSearchResultItem> Value { get; set; }
    }
}
