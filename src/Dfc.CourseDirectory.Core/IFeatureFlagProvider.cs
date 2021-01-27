using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core
{
    public interface IFeatureFlagProvider
    {
        IReadOnlyCollection<string> GetFeatureFlags();
    }
}
