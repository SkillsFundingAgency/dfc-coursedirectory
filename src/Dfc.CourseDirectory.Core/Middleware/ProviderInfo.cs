using System;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.Middleware
{
    public class ProviderInfo
    {
        public Guid ProviderId { get; set; }
        public int Ukprn { get; set; }
        public ProviderType ProviderType { get; set; }
        public string ProviderName { get; set; }
    }
}
