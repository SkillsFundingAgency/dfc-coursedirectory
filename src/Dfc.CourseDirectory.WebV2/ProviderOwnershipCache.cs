using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
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

        public async Task<Guid?> GetProviderForApprenticeship(Guid apprenticeshipId)
        {
            var cacheKey = GetCacheKey(apprenticeshipId);

            if (!_cache.TryGetValue<Guid?>(cacheKey, out var providerId))
            {
                // Cosmos document doesn't stored ProviderID on the apprenticeship but UKPRN
                // so we need two lookups here :-/

                var maybeUkprn = await _cosmosDbQueryDispatcher.ExecuteQuery(
                    new GetProviderUkprnForApprenticeship() { ApprenticeshipId = apprenticeshipId });

                if (maybeUkprn.Value is int)
                {
                    var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                        new GetProviderByUkprn() { Ukprn = maybeUkprn.AsT1 });

                    providerId = provider.Id;
                }
                else
                {
                    providerId = null;
                }

                _cache.Set(cacheKey, providerId);
            }

            return providerId;
        }

        public void OnApprenticeshipDeleted(Guid apprenticeshipId)
        {
            var cacheKey = GetCacheKey(apprenticeshipId);
            _cache.Remove(cacheKey);
        }

        private static string GetCacheKey(Guid apprenticeshipId) => $"apprenticeship-ukprns:{apprenticeshipId}";
    }
}
