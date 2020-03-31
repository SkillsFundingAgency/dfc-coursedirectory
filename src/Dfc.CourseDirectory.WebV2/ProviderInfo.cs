using System;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2
{
    public class ProviderInfo
    {
        public Guid ProviderId { get; set; }
        public int Ukprn { get; set; }
        public ProviderType ProviderType { get; set; }
        public string ProviderName { get; set; }
    }
}
