using System;
using System.Collections.Generic;

namespace Dfc.ProviderPortal.FindACourse.ApiModels
{
    public class ProviderSearchResponse
    {
        public string ODataContext { get; set; }
        public int? ODataCount { get; set; }
        public dynamic SearchFacets { get; set; } //ProviderSearchFacets SearchFacets { get; set; }
        public IEnumerable<ProviderSearchResponseItem> Value { get; set; }
    }

    public class ProviderSearchResponseItem
    {
        public Guid id { get; set; }
        public string Name { get; set; }
        public string Postcode { get; set; }
        public string Town { get; set; }
        public string Region { get; set; }
        public string UKPRN { get; set; }
        public string Status { get; set; }
        public string ProviderStatus { get; set; }
        public string CourseDirectoryName { get; set; }
        public string TradingName { get; set; }
        public string ProviderAlias { get; set; }
    }
}
