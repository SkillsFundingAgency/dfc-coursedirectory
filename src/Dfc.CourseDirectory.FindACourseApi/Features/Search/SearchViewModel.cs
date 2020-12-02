using System.Collections.Generic;

namespace Dfc.CourseDirectory.FindACourseApi.Features.Search
{
    public class SearchViewModel
    {
        public IReadOnlyDictionary<string, IEnumerable<FacetCountResultViewModel>> Facets { get; set; }
        public IReadOnlyCollection<SearchResultViewModel> Results { get; set; }
        public int Total { get; set; }
        public int Limit { get; set; }
        public int Start { get; set; }
    }
}
