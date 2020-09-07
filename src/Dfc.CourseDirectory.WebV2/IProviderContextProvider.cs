using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2
{
    public interface IProviderContextProvider
    {
        void AssignLegacyProviderContext();
        Task<ProviderContext> GetProviderContext();
        void SetProviderContext(ProviderContext providerContext);
    }
}
