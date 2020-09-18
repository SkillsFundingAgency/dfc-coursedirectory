using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.Search
{
    public class SearchResult<TResult>
    {
        public IReadOnlyCollection<TResult> Results { get; set; }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<object, long?>> Facets { get; set; }

        public long? TotalCount { get; set; }
    }
}