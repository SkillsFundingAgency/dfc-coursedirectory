using System.Collections.Generic;
using Dfc.CourseDirectory.FindACourseApi.Interfaces;
using Microsoft.Azure.Search.Models;

namespace Dfc.CourseDirectory.FindACourseApi.Models
{
    public class FACSearchResult : IFACSearchResult
    {
        public int Total { get; set; }
        public int Limit { get; set; }
        public int Start { get; set; }
        public IDictionary<string, IList<FacetResult>> Facets { get; set; }
        public IEnumerable<FACSearchResultItem> Items { get; set; }
    }
}
