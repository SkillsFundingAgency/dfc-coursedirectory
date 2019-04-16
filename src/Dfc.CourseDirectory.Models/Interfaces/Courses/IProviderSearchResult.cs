
using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Models.Courses;


namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    // TODO - Provider search is in the course service for now, needs moving!
    public interface IProviderSearchResult
    {
        string ODataContext { get; set; }
        int? ODataCount { get; set; }
        dynamic SearchFacets { get; set; } //ProviderSearchFacets SearchFacets { get; set; }
        IEnumerable<ProviderSearchResultItem> Value { get; set; }
    }
}
