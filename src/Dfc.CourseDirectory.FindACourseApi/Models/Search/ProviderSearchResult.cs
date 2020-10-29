
using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.FindACourseApi.Interfaces;


namespace Dfc.CourseDirectory.FindACourseApi.Models
{
    public class ProviderSearchResult : IProviderSearchResult
    {
        public string ODataContext { get; set; }
        public int? ODataCount { get; set; }
        public dynamic SearchFacets { get; set; } //ProviderSearchFacets SearchFacets { get; set; }
        public IEnumerable<ProviderSearchResultItem> Value { get; set; }
    }
}
