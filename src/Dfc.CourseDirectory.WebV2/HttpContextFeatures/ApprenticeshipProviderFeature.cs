using System;

namespace Dfc.CourseDirectory.WebV2.HttpContextFeatures
{
    public class ApprenticeshipProviderFeature
    {
        public ApprenticeshipProviderFeature(Guid providerId)
        {
            ProviderId = providerId;
        }

        public Guid ProviderId { get; }
    }
}
