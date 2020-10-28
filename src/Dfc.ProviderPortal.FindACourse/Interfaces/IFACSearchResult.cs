using System.Collections.Generic;
using Dfc.ProviderPortal.FindACourse.Models;
using Microsoft.Azure.Search.Models;

namespace Dfc.ProviderPortal.FindACourse.Interfaces
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
