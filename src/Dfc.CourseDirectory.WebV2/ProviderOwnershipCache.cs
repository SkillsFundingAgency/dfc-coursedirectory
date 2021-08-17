using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Microsoft.Extensions.Caching.Memory;

namespace Dfc.CourseDirectory.WebV2
{
    public class ProviderOwnershipCache : IProviderOwnershipCache
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IMemoryCache _cache;

        public ProviderOwnershipCache(
            ISqlQueryDispatcher sqlQueryDispatcher,
            IMemoryCache cache)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _cache = cache;
        }

        public async Task<Guid?> GetProviderForCourse(Guid courseId)
        {
            var cacheKey = GetCourseCacheKey(courseId);

            if (!_cache.TryGetValue<Guid?>(cacheKey, out var providerId))
            {
                var course = await _sqlQueryDispatcher.ExecuteQuery(
                    new GetCourse() { CourseId = courseId });

                if (course != null)
                {
                    providerId = course.ProviderId;
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
                var apprenticeship = await _sqlQueryDispatcher.ExecuteQuery(
                    new GetApprenticeship() { ApprenticeshipId = apprenticeshipId });

                if (apprenticeship != null)
                {
                    providerId = apprenticeship.ProviderId;
                    _cache.Set(cacheKey, providerId);
                }
                else
                {
                    providerId = null;
                }
            }

            return providerId;
        }

        public async Task<Guid?> GetProviderForTLevel(Guid tLevelId)
        {
            var cacheKey = GetTLevelCacheKey(tLevelId);

            if (!_cache.TryGetValue<Guid?>(cacheKey, out var providerId))
            {
                var tLevel = await _sqlQueryDispatcher.ExecuteQuery(
                    new GetTLevel() { TLevelId = tLevelId });

                if (tLevel != null)
                {
                    providerId = tLevel.ProviderId;
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
                var venue = await _sqlQueryDispatcher.ExecuteQuery(
                    new GetVenue() { VenueId = venueId });

                if (venue != null)
                {
                    providerId = venue.ProviderId;
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

        public void OnTLevelDeleted(Guid tLevelId)
        {
            var cacheKey = GetTLevelCacheKey(tLevelId);
            _cache.Remove(cacheKey);
        }

        public void OnVenueDeleted(Guid venueId)
        {
            var cacheKey = GetVenueCacheKey(venueId);
            _cache.Remove(cacheKey);
        }

        private static string GetApprenticeshipCacheKey(Guid apprenticeshipId) =>
            $"apprenticeship-providers:{apprenticeshipId}";

        private static string GetCourseCacheKey(Guid courseId) =>
            $"course-providers:{courseId}";

        private static string GetTLevelCacheKey(Guid tLevelId) =>
            $"tlevel-providers:{tLevelId}";

        private static string GetVenueCacheKey(Guid venueId) =>
            $"venue-providers:{venueId}";
    }
}
