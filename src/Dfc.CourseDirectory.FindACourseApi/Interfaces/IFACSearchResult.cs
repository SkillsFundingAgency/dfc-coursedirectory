using System.Collections.Generic;
using Dfc.CourseDirectory.FindACourseApi.Models;
using Microsoft.Azure.Search.Models;

namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface IFACSearchResult
    {
        int Total { get; set; }
        int Limit { get; set; }
        int Start { get; set; }
        IDictionary<string, IList<FacetResult>> Facets { get; set; }
        IEnumerable<FACSearchResultItem> Items { get; set; }
    }
}
