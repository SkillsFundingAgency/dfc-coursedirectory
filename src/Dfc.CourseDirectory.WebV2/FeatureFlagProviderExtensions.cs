using System;

namespace Dfc.CourseDirectory.WebV2
{
    public static class FeatureFlagProviderExtensions
    {
        public static bool HaveFeature(this IFeatureFlagProvider featureFlagProvider, string feature) =>
            featureFlagProvider.GetFeatureFlags().Contains(feature);
    }
}
