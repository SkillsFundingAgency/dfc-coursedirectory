using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Microsoft.Extensions.Caching.Memory;

namespace Dfc.CourseDirectory.WebV2
{
    public class ProviderOwnershipCache : IProviderOwnershipCache
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IMemoryCache _cache;

        public ProviderOwnershipCache(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher, IMemoryCache cache)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _cache = cache;
        }

        public async Task<int?> GetProviderForApprenticeship(Guid apprenticeshipId)
        {
            var cacheKey = GetCacheKey(apprenticeshipId);

            if (!_cache.TryGetValue<int?>(cacheKey, out var ukprn))
            {
                ukprn = await _cosmosDbQueryDispatcher.ExecuteQuery(
                    new GetProviderUkprnForApprenticeship() { ApprenticeshipId = apprenticeshipId });

                _cache.Set(cacheKey, ukprn);
            }

            return ukprn;
        }

        public void OnApprenticeshipDeleted(Guid apprenticeshipId)
        {
            var cacheKey = GetCacheKey(apprenticeshipId);
            _cache.Remove(cacheKey);
        }

        private static string GetCacheKey(Guid apprenticeshipId) => $"apprenticeship-ukprns:{apprenticeshipId}";
    }
}
