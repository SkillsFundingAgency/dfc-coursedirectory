using System.Collections.Generic;

namespace Dfc.CourseDirectory.FindACourseApi.ApiModels
{
    public class PostcodeSearchResponse
    {
        public string ODataContext { get; set; }
        public int? ODataCount { get; set; }
        public dynamic SearchFacets { get; set; } //PostcodeSearchFacets SearchFacets { get; set; }
        public IEnumerable<dynamic> Value { get; set; } // PostcodeSearchResultItem> Value { get; set; }
    }
}
