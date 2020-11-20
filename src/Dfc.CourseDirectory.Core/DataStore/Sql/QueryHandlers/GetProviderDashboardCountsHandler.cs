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
DECLARE @ProviderUkprn INT

SELECT TOP 1 @ProviderUkprn = Ukprn FROM Pttcd.Providers WHERE ProviderId = @ProviderId

SELECT COUNT(*) FROM Pttcd.Courses c
JOIN Pttcd.CourseRuns cr ON c.CourseId = cr.CourseId
WHERE c.ProviderUkprn = @ProviderUkprn AND cr.CourseRunStatus = 1

SELECT COUNT(*) FROM Pttcd.Apprenticeships a
WHERE a.ProviderUkprn = @ProviderUkprn AND a.ApprenticeshipStatus & 1 <> 0

SELECT COUNT(*) FROM Pttcd.Venues v
WHERE v.ProviderUkprn = @ProviderUkprn AND v.VenueStatus = 1";

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
