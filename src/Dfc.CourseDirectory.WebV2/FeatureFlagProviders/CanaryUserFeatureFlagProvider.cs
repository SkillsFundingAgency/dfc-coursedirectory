using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Security;

namespace Dfc.CourseDirectory.WebV2.FeatureFlagProviders
{
    public abstract class CanaryUserFeatureFlagProvider : IFeatureFlagProvider
    {
        private readonly Lazy<IReadOnlyCollection<string>> _featureFlags;

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

            _featureFlags = new Lazy<IReadOnlyCollection<string>>(() =>
                ResolveFeatureFlags(currentUserProvider.GetCurrentUser(), innerFeatureFlagProvider.GetFeatureFlags()));
        }

        public IReadOnlyCollection<string> GetFeatureFlags()
        {
            return _featureFlags.Value;
        }

        protected abstract IReadOnlyCollection<string> ResolveFeatureFlags(AuthenticatedUserInfo user, IReadOnlyCollection<string> featureFlags);
    }
}
