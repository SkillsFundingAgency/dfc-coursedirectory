using System;

namespace Dfc.CourseDirectory.WebV2.HttpContextFeatures
{
    public class ProviderContextFeature
    {
        public ProviderContextFeature(ProviderContext providerContext)
        {
            ProviderContext = providerContext ?? throw new ArgumentNullException(nameof(providerContext));
        }

        public ProviderContext ProviderContext { get; }
    }
}
