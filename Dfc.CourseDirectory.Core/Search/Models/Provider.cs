using System;

namespace Dfc.CourseDirectory.Core.Search.Models
{
    public class Provider
    {
        public Guid ProviderId { get; set; }
        public string Ukprn { get; set; }
        public string ProviderName { get; set; }
        public int ProviderStatus { get; set; }
        public string UkrlpProviderStatusDescription { get; set; }
        public string Postcode { get; set; }
        public string Town { get; set; }
    }
}
