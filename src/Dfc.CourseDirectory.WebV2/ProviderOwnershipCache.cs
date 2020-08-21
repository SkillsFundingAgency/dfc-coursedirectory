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

        public async Task<Guid?> GetProviderForCourse(Guid courseId)
        {
            var cacheKey = GetCourseCacheKey(courseId);

            if (!_cache.TryGetValue<Guid?>(cacheKey, out var providerId))
            {
                // Cosmos document doesn't store ProviderID on the course but UKPRN
                // so we need two lookups here :-/

                var ukprn = await _cosmosDbQueryDispatcher.ExecuteQuery(
                    new GetProviderUkprnForCourse() { CourseId = courseId });

                if (ukprn.HasValue)
                {
                    var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                        new GetProviderByUkprn() { Ukprn = ukprn.Value });

                    providerId = provider.Id;

                    _cache.Set(cacheKey, providerId);
                }
                else
                {
                    providerId = null;
                }
            }

            return providerId;
        }

        public async Task<Guid?> GetProviderForApprenticeship(Guid apprenticeshipId)
        {
            var cacheKey = GetApprenticeshipCacheKey(apprenticeshipId);

            if (!_cache.TryGetValue<Guid?>(cacheKey, out var providerId))
            {
                // Cosmos document doesn't store ProviderID on the apprenticeship but UKPRN
                // so we need two lookups here :-/

                var maybeUkprn = await _cosmosDbQueryDispatcher.ExecuteQuery(
                    new GetProviderUkprnForApprenticeship() { ApprenticeshipId = apprenticeshipId });

                if (maybeUkprn.Value is int)
                {
                    var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                        new GetProviderByUkprn() { Ukprn = maybeUkprn.AsT1 });

                    providerId = provider.Id;

                    _cache.Set(cacheKey, providerId);
                }
                else
                {
                    providerId = null;
                }
            }

            return providerId;
        }

        public void OnApprenticeshipDeleted(Guid apprenticeshipId)
        {
            var cacheKey = GetApprenticeshipCacheKey(apprenticeshipId);
            _cache.Remove(cacheKey);
        }

        public void OnCourseDeleted(Guid courseId)
        {
            var cacheKey = GetCourseCacheKey(courseId);
            _cache.Remove(cacheKey);
        }

        private static string GetApprenticeshipCacheKey(Guid apprenticeshipId) =>
            $"apprenticeship-ukprns:{apprenticeshipId}";

        private static string GetCourseCacheKey(Guid courseId) =>
            $"course-ukprns:{courseId}";
    }
}
