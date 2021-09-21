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
SELECT COUNT(*)
FROM Pttcd.CourseRuns cr
JOIN Pttcd.Courses c ON c.CourseId = cr.CourseId
WHERE c.ProviderId = @ProviderId
AND cr.CourseRunStatus = {(int)CourseStatus.Live}

SELECT a.ApprenticeshipStatus, COUNT(*) as Count
FROM Pttcd.Apprenticeships a
WHERE a.ProviderId = @ProviderId
GROUP BY a.ApprenticeshipStatus

SELECT COUNT(*)
FROM Pttcd.TLevels t
WHERE t.ProviderId = @{nameof(query.ProviderId)}
AND t.TLevelStatus = {(int)TLevelStatus.Live}

SELECT COUNT(*)
FROM Pttcd.Venues v
WHERE v.ProviderId = @ProviderId
AND v.VenueStatus = {(int)VenueStatus.Live}

SELECT COUNT(*)
FROM Pttcd.CourseRuns cr
JOIN Pttcd.Courses c ON c.CourseId = cr.CourseId
WHERE c.ProviderId = @ProviderId
AND cr.CourseRunStatus = {(int)CourseStatus.Live}
AND cr.StartDate < @{nameof(query.Date)}

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
AND cu.ProviderId = @{ nameof(query.ProviderId)}

SELECT COUNT(*)
FROM Pttcd.ApprenticeshipUploads au
INNER JOIN Pttcd.ApprenticeshipUploadRows ar
ON au.ApprenticeshipUploadId = ar.ApprenticeshipUploadId
WHERE au.UploadStatus IN ({(int)UploadStatus.ProcessedWithErrors}, { (int)UploadStatus.ProcessedSuccessfully})
AND ar.ApprenticeshipUploadRowStatus = 0
AND au.ProviderId = @{ nameof(query.ProviderId)}";

            using (var reader = await transaction.Connection.QueryMultipleAsync(sql, query, transaction))
            {
                var courseRunCount = reader.ReadSingle<int>();
                var apprenticeshipCounts = reader.Read().ToDictionary(r => (ApprenticeshipStatus)r.ApprenticeshipStatus, r => (int)r.Count);
                var tLevelCount = reader.ReadSingle<int>();
                var venueCount = reader.ReadSingle<int>();
                var pastStartDateCourseRunCount = reader.ReadSingle<int>();
                var unpublishedVenueCount = reader.ReadSingle<int>();
                var unpublishedCourseCount = reader.ReadSingle<int>();
                var unpublishedApprenticeshipCount = reader.ReadSingle<int>();

                return new DashboardCounts
                {
                    CourseRunCount = courseRunCount,
                    ApprenticeshipCounts = apprenticeshipCounts,
                    TLevelCount = tLevelCount,
                    VenueCount = venueCount,
                    PastStartDateCourseRunCount = pastStartDateCourseRunCount,
                    UnpublishedVenueCount = unpublishedVenueCount,
                    UnpublishedCourseCount = unpublishedCourseCount,
                    UnpublishedApprenticeshipCount = unpublishedApprenticeshipCount
                };
            }
        }
    }
}
