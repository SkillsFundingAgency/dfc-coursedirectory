using System;

namespace Dfc.CourseDirectory.WebV2
{
    public class ProviderContext
    {
        public ProviderContext(ProviderInfo providerInfo, bool isFallback = false)
        {
            ProviderInfo = providerInfo ?? throw new ArgumentNullException(nameof(providerInfo));
            IsFallback = isFallback;
        }

        public ProviderInfo ProviderInfo { get; }

        public bool IsFallback { get; }
    }
}
