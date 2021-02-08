using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Security;

namespace Dfc.CourseDirectory.WebV2
{
    public class TLevelsBetaCanaryUserFeatureFlagProvider : CanaryUserFeatureFlagProvider
    {
        public TLevelsBetaCanaryUserFeatureFlagProvider(IFeatureFlagProvider innerFeatureFlagProvider, ICurrentUserProvider currentUserProvider)
            :base(innerFeatureFlagProvider, currentUserProvider)
        {
        }

        protected override IReadOnlyCollection<string> ResolveFeatureFlags(AuthenticatedUserInfo user, IReadOnlyCollection<string> featureFlags)
        {
            if (!featureFlags.Contains(FeatureFlags.TLevels)
                && featureFlags.Contains(FeatureFlags.TLevelsBeta)
                && (user?.IsAdmin ?? false))
            {
                return new List<string>(featureFlags)
                {
                    FeatureFlags.TLevels
                };
            }

            return featureFlags;
        }
    }
}
