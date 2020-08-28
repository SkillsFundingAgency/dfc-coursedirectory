using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2
{
    public interface IProviderContextProvider
    {
        Task<ProviderInfo> GetProviderContext();
        void SetProviderContext(ProviderInfo providerInfo);
    }
}
