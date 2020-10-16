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
                var ukprn = await _cosmosDbQueryDispatcher.ExecuteQuery(
                    new GetProviderUkprnForCourse() { CourseId = courseId });

                if (ukprn.HasValue)
                {
                    providerId = await GetProviderIdByUkprn(ukprn);
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
                var ukprn = await _cosmosDbQueryDispatcher.ExecuteQuery(
                    new GetProviderUkprnForApprenticeship() { ApprenticeshipId = apprenticeshipId });

                if (ukprn.HasValue)
                {
                    providerId = await GetProviderIdByUkprn(ukprn);
                    _cache.Set(cacheKey, providerId);
                }
                else
                {
                    providerId = null;
                }
            }

            return providerId;
        }

        public async Task<Guid?> GetProviderForVenue(Guid venueId)
        {
            var cacheKey = GetVenueCacheKey(venueId);

            if (!_cache.TryGetValue<Guid?>(cacheKey, out var providerId))
            {
                var venue = await _cosmosDbQueryDispatcher.ExecuteQuery(
                    new GetVenueById() { VenueId = venueId });

                if (venue != null)
                {
                    providerId = await GetProviderIdByUkprn(venue.Ukprn);
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

        public void OnVenueDeleted(Guid venueId)
        {
            var cacheKey = GetVenueCacheKey(venueId);
            _cache.Remove(cacheKey);
        }

        private static string GetApprenticeshipCacheKey(Guid apprenticeshipId) =>
            $"apprenticeship-ukprns:{apprenticeshipId}";

        private static string GetCourseCacheKey(Guid courseId) =>
            $"course-ukprns:{courseId}";

        private static string GetVenueCacheKey(Guid venueId) =>
            $"venue-ukprns:{venueId}";

        private async Task<Guid> GetProviderIdByUkprn(int? ukprn) =>
            (await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderByUkprn() { Ukprn = ukprn.Value })).Id;
    }
}
