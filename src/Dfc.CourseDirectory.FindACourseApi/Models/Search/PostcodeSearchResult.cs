
using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.FindACourseApi.Interfaces;


namespace Dfc.CourseDirectory.FindACourseApi.Models
{
    public class PostcodeSearchResult : IPostcodeSearchResult
    {
        public string ODataContext { get; set; }
        public int? ODataCount { get; set; }
        public dynamic SearchFacets { get; set; } //PostcodeSearchFacets SearchFacets { get; set; }
        public IEnumerable<dynamic> Value { get; set; } // PostcodeSearchResultItem> Value { get; set; }
}
}
