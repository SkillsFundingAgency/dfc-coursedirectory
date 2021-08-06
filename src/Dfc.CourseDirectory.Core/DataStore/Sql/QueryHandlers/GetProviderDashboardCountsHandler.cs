using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetProviderDashboardCountsHandler :
        ISqlQueryHandler<GetProviderDashboardCounts, DashboardCounts>
    {
        public async Task<DashboardCounts> Execute(
            SqlTransaction transaction,
            GetProviderDashboardCounts query)
        {
            var sql = @$"
DECLARE @ProviderUkprn INT

SELECT TOP 1 @ProviderUkprn = Ukprn FROM Pttcd.Providers WHERE ProviderId = @{nameof(query.ProviderId)}

SELECT      cr.CourseRunStatus, COUNT(*) as Count
FROM        Pttcd.CourseRuns cr
INNER JOIN  Pttcd.Courses c ON c.CourseId = cr.CourseId
WHERE       c.ProviderUkprn = @ProviderUkprn
GROUP BY    cr.CourseRunStatus

SELECT      a.ApprenticeshipStatus, COUNT(*) as Count
FROM        Pttcd.Apprenticeships a
WHERE       a.ProviderUkprn = @ProviderUkprn
GROUP BY    a.ApprenticeshipStatus

SELECT      t.TLevelStatus, COUNT(*) as Count
FROM        Pttcd.TLevels t
WHERE       t.ProviderId = @{nameof(query.ProviderId)}
GROUP BY    t.TLevelStatus

SELECT      COUNT(*)
FROM        Pttcd.Venues v
WHERE       v.ProviderId = @ProviderId
AND         v.VenueStatus = 1

SELECT      COUNT(*)
FROM        Pttcd.CourseRuns cr
INNER JOIN  Pttcd.Courses c ON c.CourseId = cr.CourseId
WHERE       c.ProviderId = @ProviderId
AND         c.CourseStatus = c.CourseStatus & {(int)CourseStatus.Live}
AND         cr.StartDate < @{nameof(query.Date)}

SELECT COUNT(*)
FROM Pttcd.VenueUploads vu
INNER JOIN Pttcd.VenueUploadRows vr
ON vu.VenueUploadId = vr.VenueUploadId
WHERE vu.UploadStatus IN ({(int)UploadStatus.ProcessedWithErrors}, {(int)UploadStatus.ProcessedSuccessfully})
AND vr.VenueUploadRowStatus = 0 
AND vu.ProviderId = @{ nameof(query.ProviderId)}

SELECT COUNT(*)
FROM Pttcd.CourseUploads cu
INNER JOIN Pttcd.CourseUploadRows cr
ON cu.CourseUploadId = cr.CourseUploadId
WHERE cu.UploadStatus IN ({(int)UploadStatus.ProcessedWithErrors}, { (int)UploadStatus.ProcessedSuccessfully})
AND cr.CourseUploadRowStatus = 0
AND cu.ProviderId = @{ nameof(query.ProviderId)}";

            using (var reader = await transaction.Connection.QueryMultipleAsync(sql, query, transaction))
            {
                var courseRunCounts = reader.Read().ToDictionary(r => (CourseStatus)r.CourseRunStatus, r => (int)r.Count);
                var apprenticeshipCounts = reader.Read().ToDictionary(r => (ApprenticeshipStatus)r.ApprenticeshipStatus, r => (int)r.Count);
                var tLevelCounts = reader.Read().ToDictionary(r => (TLevelStatus)r.TLevelStatus, r => (int)r.Count);
                var venueCount = reader.ReadSingle<int>();
                var pastStartDateCourseRunCount = reader.ReadSingle<int>();
                var unpublishedVenueCount = reader.ReadSingle<int>();
                var unpublishedCourseCount = reader.ReadSingle<int>();

                return new DashboardCounts
                {
                    CourseRunCounts = courseRunCounts,
                    ApprenticeshipCounts = apprenticeshipCounts,
                    TLevelCounts = tLevelCounts,
                    VenueCount = venueCount,
                    PastStartDateCourseRunCount = pastStartDateCourseRunCount,
                    UnpublishedVenueCount = unpublishedVenueCount,
                    UnpublishedCourseCount = unpublishedCourseCount
                };
            }
        }
    }
}
