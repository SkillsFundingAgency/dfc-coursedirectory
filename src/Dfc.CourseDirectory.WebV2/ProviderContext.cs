using System;

namespace Dfc.CourseDirectory.WebV2
{
    public class ProviderContext
    {
        public ProviderContext(ProviderInfo providerInfo)
        {
            ProviderInfo = providerInfo ?? throw new ArgumentNullException(nameof(providerInfo));
        }

        public ProviderInfo ProviderInfo { get; }
    }
}
