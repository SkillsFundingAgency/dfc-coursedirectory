using System;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2
{
    public interface IProviderInfoCache
    {
        Task<ProviderInfo> GetProviderInfo(Guid providerId);
        Task Remove(Guid providerId);

        // LEGACY SUPPORT
        Task<Guid?> GetProviderIdForUkprn(int ukprn);
    }
}
