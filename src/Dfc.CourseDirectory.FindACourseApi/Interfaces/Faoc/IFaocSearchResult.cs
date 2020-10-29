using System.Collections.Generic;
using Dfc.CourseDirectory.FindACourseApi.Models;
using Dfc.CourseDirectory.FindACourseApi.Models.Search.Faoc;
using Microsoft.Azure.Search.Models;

namespace Dfc.CourseDirectory.FindACourseApi.Interfaces.Faoc
{
    public interface IFaocSearchResult
    {
        int Total { get; set; }
        int Limit { get; set; }
        int Start { get; set; }
        IDictionary<string, IList<FacetResult>> Facets { get; set; }
        IEnumerable<FaocSearchResultItem> Items { get; set; }
    }
}