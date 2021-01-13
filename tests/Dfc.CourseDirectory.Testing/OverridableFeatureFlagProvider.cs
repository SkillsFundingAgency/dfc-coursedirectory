using System.Collections.Generic;
using Dfc.CourseDirectory.Core;

namespace Dfc.CourseDirectory.Testing
{
    public class OverridableFeatureFlagProvider : IFeatureFlagProvider
    {
        private readonly IFeatureFlagProvider _inner;
        private ISet<string> _overridenFeatures;

        public OverridableFeatureFlagProvider(IFeatureFlagProvider inner)
        {
            _inner = inner;
        }

        public ISet<string> GetFeatureFlags()
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
            _overridenFeatures = new HashSet<string>(features);
        }
    }
}
