using System.Collections.Generic;
using Dfc.ProviderPortal.FindACourse.Interfaces.Faoc;
using Microsoft.Azure.Search.Models;

namespace Dfc.ProviderPortal.FindACourse.Models.Search.Faoc
{
    public class FaocSearchResult : IFaocSearchResult
    {
        public int Total { get; set; }
        public int Limit { get; set; }
        public int Start { get; set; }
        public IDictionary<string, IList<FacetResult>> Facets { get; set; }
        public IEnumerable<FaocSearchResultItem> Items { get; set; }
    }
}