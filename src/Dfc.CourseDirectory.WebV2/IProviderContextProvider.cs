namespace Dfc.CourseDirectory.WebV2
{
    public interface IProviderContextProvider
    {
        void AssignLegacyProviderContext();
        ProviderContext GetProviderContext(bool withLegacyFallback = false);
        void SetProviderContext(ProviderContext providerContext);
    }
}
