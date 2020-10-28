using System.Collections.Generic;

namespace Dfc.ProviderPortal.FindACourse.ApiModels.Faoc
{
    public class OnlineCourseSearchResponse : IPagedResponse
    {
        public IDictionary<string, IEnumerable<FacetCountResult>> Facets { get; set; }
        public IEnumerable<OnlineCourseSearchResponseItem> Results { get; set; }
        public int Total { get; set; }
        public int Limit { get; set; }
        public int Start { get; set; }
    }
}