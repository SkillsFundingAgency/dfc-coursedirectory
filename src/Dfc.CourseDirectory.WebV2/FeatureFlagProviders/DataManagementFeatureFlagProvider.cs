using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.Extensions.Configuration;

namespace Dfc.CourseDirectory.WebV2.FeatureFlagProviders
{
    public class DataManagementFeatureFlagProvider : IFeatureFlagProvider
    {
        private readonly IFeatureFlagProvider _innerFeatureFlagProvider;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly HashSet<int> _betaTestingProviderUkprns;

        public DataManagementFeatureFlagProvider(
            IFeatureFlagProvider innerFeatureFlagProvider,
            ICurrentUserProvider currentUserProvider,
            IConfiguration configuration)
        {
            _innerFeatureFlagProvider = innerFeatureFlagProvider;
            _currentUserProvider = currentUserProvider;

            _betaTestingProviderUkprns = new HashSet<int>(
                (configuration["DataManagementBetaTestingProviders"] ?? string.Empty)
                    .Split(';', System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(ukprn => int.Parse(ukprn)));
        }

        public ConfiguredFeaturesCollection GetFeatureFlags()
        {
            var features = _innerFeatureFlagProvider.GetFeatureFlags();

            var currentUser = _currentUserProvider.GetCurrentUser();

            // All admins get VenuesDataManagement
            if (currentUser.IsAdmin)
            {
                features = features.With(FeatureFlags.VenuesDataManagement);
            }

            // If the user is bound to a provider on the beta testers list, they get VenuesDataManagement
            if (currentUser.CurrentProviderUkprn.HasValue &&
                _betaTestingProviderUkprns.Contains(currentUser.CurrentProviderUkprn.Value))
            {
                features = features.With(FeatureFlags.VenuesDataManagement);
            }

            // If any of '{Courses|Venues}DataManagement' features are defined
            // then also define the root 'DataManagement' feature
            if (features.HaveFeature(FeatureFlags.CoursesDataManagement) ||
                features.HaveFeature(FeatureFlags.VenuesDataManagement))
            {
                features = features.With(FeatureFlags.DataManagement);
            }

            return features;
        }
    }
}
