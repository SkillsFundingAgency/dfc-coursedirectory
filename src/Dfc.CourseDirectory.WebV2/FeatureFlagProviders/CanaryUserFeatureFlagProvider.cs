using System;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Security;

namespace Dfc.CourseDirectory.WebV2.FeatureFlagProviders
{
    public abstract class CanaryUserFeatureFlagProvider : IFeatureFlagProvider
    {
        private readonly IFeatureFlagProvider _innerFeatureFlagProvider;
        private readonly ICurrentUserProvider _currentUserProvider;

        public CanaryUserFeatureFlagProvider(
            IFeatureFlagProvider innerFeatureFlagProvider,
            ICurrentUserProvider currentUserProvider)
        {
            if (innerFeatureFlagProvider is null)
            {
                throw new ArgumentNullException(nameof(innerFeatureFlagProvider));
            }

            if (currentUserProvider is null)
            {
                throw new ArgumentNullException(nameof(currentUserProvider));
            }

            _innerFeatureFlagProvider = innerFeatureFlagProvider;
            _currentUserProvider = currentUserProvider;
        }

        public ConfiguredFeaturesCollection GetFeatureFlags() =>
            ResolveFeatureFlags(_currentUserProvider.GetCurrentUser(), _innerFeatureFlagProvider.GetFeatureFlags());

        protected abstract ConfiguredFeaturesCollection ResolveFeatureFlags(
            AuthenticatedUserInfo user,
            ConfiguredFeaturesCollection featureFlags);
    }
}
