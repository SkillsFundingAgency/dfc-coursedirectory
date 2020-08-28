using System;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2
{
    public static class ProviderInfoCacheExtensions
    {
        public static async Task<ProviderInfo> GetProviderInfoForUkprn(this IProviderInfoCache providerInfoCache, int ukprn)
        {
            if (providerInfoCache == null)
            {
                throw new ArgumentNullException(nameof(providerInfoCache));
            }

            var providerId = await providerInfoCache.GetProviderIdForUkprn(ukprn);

            if (providerId == null)
            {
                return null;
            }

            return await providerInfoCache.GetProviderInfo(providerId.Value);
        }
    }
}
