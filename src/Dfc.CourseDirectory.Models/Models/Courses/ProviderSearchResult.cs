
using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Interfaces.Courses;


namespace Dfc.CourseDirectory.Models.Models.Courses
{
    // TODO - Provider search is in the course service for now, needs moving!
    public class ProviderSearchResult : IProviderSearchResult
    {
        public string ODataContext { get; set; }
        public int? ODataCount { get; set; }
        public dynamic SearchFacets { get; set; } //ProviderSearchFacets SearchFacets { get; set; }
        public IEnumerable<ProviderSearchResultItem> Value { get; set; }
    }
}
