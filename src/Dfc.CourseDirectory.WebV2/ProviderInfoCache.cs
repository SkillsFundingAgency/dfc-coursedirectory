using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Microsoft.Extensions.Caching.Memory;

namespace Dfc.CourseDirectory.WebV2
{
    public class ProviderInfoCache : IProviderInfoCache
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IMemoryCache _cache;

        public ProviderInfoCache(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher, IMemoryCache cache)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _cache = cache;
        }

        public async Task<ProviderInfo> GetProviderInfo(int ukprn)
        {
            var cacheKey = GetCacheKey(ukprn);

            if (!_cache.TryGetValue<ProviderInfo>(cacheKey, out var providerInfo))
            {
                var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                    new GetProviderByUkprn() { Ukprn = ukprn });

                if (provider == null)
                {
                    return null;
                }

                providerInfo = new ProviderInfo()
                {
                    ProviderId = provider.Id,
                    Ukprn = ukprn
                };

                _cache.Set(cacheKey, providerInfo);
            }

            return providerInfo;
        }

        private static string GetCacheKey(int ukprn) => $"provider-info:{ukprn}";
    }
}
