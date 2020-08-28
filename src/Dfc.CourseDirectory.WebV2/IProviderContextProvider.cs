using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2
{
    public interface IProviderContextProvider
    {
        Task<ProviderContext> GetProviderContext();
        void SetProviderContext(ProviderContext providerContext);
    }
}
