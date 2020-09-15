namespace Dfc.CourseDirectory.WebV2
{
    public interface IProviderContextProvider
    {
        void AssignLegacyProviderContext();
        ProviderContext GetProviderContext();
        void SetProviderContext(ProviderContext providerContext);
    }
}
