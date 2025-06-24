using System;

namespace Dfc.CourseDirectory.Core.Middleware
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
