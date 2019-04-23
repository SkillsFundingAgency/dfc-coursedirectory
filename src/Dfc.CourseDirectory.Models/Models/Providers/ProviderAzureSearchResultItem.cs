
using System;


namespace Dfc.CourseDirectory.Models.Models.Providers
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
