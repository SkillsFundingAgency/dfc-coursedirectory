
using System;


namespace Dfc.CourseDirectory.Web.ViewComponents.ProviderSearchResult
{
    public class ProviderAzureSearchResultItem
    {
        public Guid id { get; set; }
        public string Name { get; set; }
        public string Postcode { get; set; }
        public string Town { get; set; }
        public string Region { get; set; }
        public string ProviderId { get; set; }
    }
}
