using Dfc.CourseDirectory.Core;

namespace Dfc.CourseDirectory.WebV2.FeatureFlagProviders
{
    public class DataManagementFeatureFlagProvider : IFeatureFlagProvider
    {
        private readonly IFeatureFlagProvider _innerFeatureFlagProvider;

        public DataManagementFeatureFlagProvider(IFeatureFlagProvider innerFeatureFlagProvider)
        {
            _innerFeatureFlagProvider = innerFeatureFlagProvider;
        }

        public ConfiguredFeaturesCollection GetFeatureFlags()
        {
            var features = _innerFeatureFlagProvider.GetFeatureFlags();

            if (features.HaveFeature(FeatureFlags.CoursesDataManagement) ||
                features.HaveFeature(FeatureFlags.VenuesDataManagement))
            {
                features = features.With(FeatureFlags.DataManagement);
            }

            return features;
        }
    }
}
