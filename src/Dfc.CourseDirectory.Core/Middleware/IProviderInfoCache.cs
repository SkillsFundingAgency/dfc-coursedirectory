using System;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.Middleware
{
    public interface IProviderInfoCache
    {
        Task<ProviderInfo> GetProviderInfo(Guid providerId);
        Task Remove(Guid providerId);

        // LEGACY SUPPORT
        Task<Guid?> GetProviderIdForUkprn(int ukprn);
    }
}
