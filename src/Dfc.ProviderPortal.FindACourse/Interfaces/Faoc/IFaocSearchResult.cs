using System.Collections.Generic;
using Dfc.ProviderPortal.FindACourse.Models;
using Dfc.ProviderPortal.FindACourse.Models.Search.Faoc;
using Microsoft.Azure.Search.Models;

namespace Dfc.ProviderPortal.FindACourse.Interfaces.Faoc
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