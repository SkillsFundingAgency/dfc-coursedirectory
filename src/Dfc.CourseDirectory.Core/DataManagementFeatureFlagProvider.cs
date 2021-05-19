using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Core
{
    public class DataManagementFeatureFlagProvider : IFeatureFlagProvider
    {
        private readonly IFeatureFlagProvider _innerFeatureFlagProvider;

        public DataManagementFeatureFlagProvider(IFeatureFlagProvider innerFeatureFlagProvider)
        {
            _innerFeatureFlagProvider = innerFeatureFlagProvider;
        }

        public IReadOnlyCollection<string> GetFeatureFlags()
        {
            var features = _innerFeatureFlagProvider.GetFeatureFlags().ToList();

            if (_innerFeatureFlagProvider.HaveFeature(FeatureFlags.CoursesDataManagement) ||
                _innerFeatureFlagProvider.HaveFeature(FeatureFlags.VenuesDataManagement))
            {
                features.Add(FeatureFlags.DataManagement);
            }

            return features.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }
    }
}
