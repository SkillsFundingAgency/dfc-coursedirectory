using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetProviderDashboardCountsHandler :
        ISqlQueryHandler<GetProviderDashboardCounts, (int CourseRunCount, int ApprenticeshipCount, int VenueCount)>
    {
        public async Task<(int CourseRunCount, int ApprenticeshipCount, int VenueCount)> Execute(
            SqlTransaction transaction,
            GetProviderDashboardCounts query)
        {
            var sql = @"
SELECT COUNT(*) FROM Pttcd.Courses c
JOIN Pttcd.CourseRuns cr ON c.CourseId = cr.CourseId
JOIN Pttcd.Providers p ON c.ProviderUkprn = p.Ukprn
WHERE p.ProviderId = @ProviderId AND cr.CourseRunStatus = 1

SELECT COUNT(*) FROM Pttcd.Apprenticeships a
JOIN Pttcd.Providers p ON a.ProviderUkprn = p.Ukprn
WHERE p.ProviderId = @ProviderId AND a.ApprenticeshipStatus & 1 <> 0

SELECT COUNT(*) FROM Pttcd.Venues v
JOIN Pttcd.Providers p ON v.ProviderUkprn = p.Ukprn
WHERE p.ProviderId = @ProviderId AND v.VenueStatus & 1 <> 0";

            using (var reader = await transaction.Connection.QueryMultipleAsync(sql, query, transaction))
            {
                var courseRunCount = reader.ReadSingle<int>();
                var apprenticeshipCount = reader.ReadSingle<int>();
                var venueCount = reader.ReadSingle<int>();

                return (courseRunCount, apprenticeshipCount, venueCount);
            }
        }
    }
}
