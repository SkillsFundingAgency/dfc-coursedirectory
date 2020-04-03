using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.WebV2
{
    public class ProviderInfoCache : IProviderInfoCache
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IDistributedCache _cache;

        public ProviderInfoCache(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher, IDistributedCache cache)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _cache = cache;
        }

        public async Task<ProviderInfo> GetProviderInfo(Guid providerId)
        {
            var cacheKey = GetCacheKey(providerId);

            ProviderInfo result = null;

            var cacheResult = await _cache.GetStringAsync(cacheKey);

            if (cacheResult != null)
            {
                try
                {
                    result = Deserialize(cacheResult);
                }
                catch (JsonSerializationException)
                {
                    // If we make a breaking change to ProviderInfo serialization could fail;
                    // force a reload in that case
                }
            }

            if (result == null)
            {
                var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                    new GetProviderById() { ProviderId = providerId });

                if (provider == null)
                {
                    return null;
                }

                result = new ProviderInfo()
                {
                    ProviderId = provider.Id,
                    Ukprn = provider.Ukprn,
                    ProviderType = provider.ProviderType,
                    ProviderName = provider.ProviderName
                };

                await _cache.SetStringAsync(cacheKey, Serialize(result));
            }

            return result;
        }

        public async Task<Guid?> GetProviderIdForUkprn(int ukprn)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetProviderByUkprn() { Ukprn = ukprn });

            if (provider != null)
            {
                return provider.Id;
            }
            else
            {
                return null;
            }
        }

        private static ProviderInfo Deserialize(string value) => JsonConvert.DeserializeObject<ProviderInfo>(value);

        private static string GetCacheKey(Guid providerId) => $"provider-info:{providerId}";

        private static string Serialize(ProviderInfo value) => JsonConvert.SerializeObject(value);
    }
}
