namespace Dfc.CourseDirectory.Core.Middleware
{
    public interface IProviderContextProvider
    {
        void AssignLegacyProviderContext();
        ProviderContext GetProviderContext(bool withLegacyFallback = false);
        void SetProviderContext(ProviderContext providerContext);
    }
}
