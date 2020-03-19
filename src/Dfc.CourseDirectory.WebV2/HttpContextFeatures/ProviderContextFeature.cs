using System;

namespace Dfc.CourseDirectory.WebV2.HttpContextFeatures
{
    public class ProviderContextFeature
    {
        public ProviderContextFeature(ProviderInfo providerInfo)
        {
            if (providerInfo == null)
            {
                throw new ArgumentNullException(nameof(providerInfo));
            }

            ProviderInfo = providerInfo;
        }

        public ProviderInfo ProviderInfo { get; }
    }
}
