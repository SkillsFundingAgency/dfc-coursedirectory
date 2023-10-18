using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Components;

namespace Dfc.CourseDirectory.WebV2
{
    public class ProviderInfoCache : IProviderInfoCache
    {
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;
        private readonly IDistributedCache _cache;
        private readonly ILogger<ProviderInfoCache> _logger;

        private static readonly TimeSpan _slidingExpiration = TimeSpan.FromHours(1);

        public ProviderInfoCache(
           ISqlQueryDispatcherFactory sqlQueryDispatcherFactory,
            IDistributedCache cache,
            ILoggerFactory loggerFactory)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
            _cache = cache;
            _logger = loggerFactory.CreateLogger<ProviderInfoCache>();
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
                    _logger.LogDebug($"Got ProviderInfo from cache for provider {providerId}.");
                }
                catch (JsonSerializationException ex)
                {
                    // If we make a breaking change to ProviderInfo serialization could fail;
                    // force a reload in that case
                    _logger.LogWarning(ex, $"Failed deserializing ProviderInfo for provider {providerId}.");
                }
            }

            if (result == null)
            {
                using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();
                var provider = await dispatcher.ExecuteQuery(
                    new GetProviderById() { ProviderId = providerId });

                if (provider == null)
                {
                    return null;
                }

                result = new ProviderInfo()
                {
                    ProviderId = provider.ProviderId,
                    Ukprn = provider.Ukprn,
                    ProviderType = provider.ProviderType,
                    ProviderName = provider.ProviderName
                };

                var entryOptions = new DistributedCacheEntryOptions() { SlidingExpiration = _slidingExpiration };
                await _cache.SetStringAsync(cacheKey, Serialize(result), entryOptions);
                _logger.LogDebug($"Added ProviderInfo to cache for provider {providerId}.");
            }

            return result;
        }

        public async Task<Guid?> GetProviderIdForUkprn(int ukprn)
        {
            using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();
            var provider = await dispatcher.ExecuteQuery(
                new GetProviderByUkprn() { Ukprn = ukprn });

            if (provider != null)
            {
                return provider.ProviderId;
            }
            else
            {
                return null;
            }
        }

        public Task Remove(Guid providerId) => _cache.RemoveAsync(GetCacheKey(providerId));

        private static ProviderInfo Deserialize(string value) => JsonConvert.DeserializeObject<ProviderInfo>(value);

        private static string GetCacheKey(Guid providerId) => $"provider-info:{providerId}";

        private static string Serialize(ProviderInfo value) => JsonConvert.SerializeObject(value);
    }
}
