using Dfc.CourseDirectory.Core;

namespace Dfc.CourseDirectory.Testing
{
    public class OverridableFeatureFlagProvider : IFeatureFlagProvider
    {
        private readonly IFeatureFlagProvider _inner;
        private ConfiguredFeaturesCollection _overridenFeatures;

        public OverridableFeatureFlagProvider(IFeatureFlagProvider inner)
        {
            _inner = inner;
        }

        public ConfiguredFeaturesCollection GetFeatureFlags()
        {
            if (_overridenFeatures != null)
            {
                return _overridenFeatures;
            }
            else
            {
                return _inner.GetFeatureFlags();
            }
        }

        public void Reset()
        {
            _overridenFeatures = null;
        }

        public void SetFeatures(params string[] features)
        {
            _overridenFeatures = new ConfiguredFeaturesCollection(features);
        }
    }
}
