using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2
{
    public interface IFeatureFlagProvider
    {
        ISet<string> GetFeatureFlags();
    }
}
