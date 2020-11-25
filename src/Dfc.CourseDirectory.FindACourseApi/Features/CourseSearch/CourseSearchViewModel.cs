using System.Collections.Generic;

namespace Dfc.CourseDirectory.FindACourseApi.Features.CourseSearch
{
    public class CourseSearchViewModel
    {
        public IReadOnlyDictionary<string, IEnumerable<FacetCountResultViewModel>> Facets { get; set; }
        public IReadOnlyCollection<CourseSearchResultViewModel> Results { get; set; }
        public int Total { get; set; }
        public int Limit { get; set; }
        public int Start { get; set; }
    }
}
