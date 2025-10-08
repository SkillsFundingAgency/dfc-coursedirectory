namespace Dfc.CourseDirectory.Core
{
    public interface IFeatureFlagProvider
    {
        ConfiguredFeaturesCollection GetFeatureFlags();
    }
}
