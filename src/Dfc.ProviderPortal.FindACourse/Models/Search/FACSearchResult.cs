using System.Collections.Generic;
using Dfc.ProviderPortal.FindACourse.Interfaces;
using Microsoft.Azure.Search.Models;

namespace Dfc.ProviderPortal.FindACourse.Models
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
