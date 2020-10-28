
using System;


namespace Dfc.ProviderPortal.FindACourse.Models
{
    public class ProviderSearchResultItem
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
