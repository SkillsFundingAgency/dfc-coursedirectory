using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetProviderDashboardCountsHandler :
        ISqlQueryHandler<GetProviderDashboardCounts, (IReadOnlyDictionary<CourseStatus, int> CourseRunCounts, int ApprenticeshipCount, int VenueCount)>
    {
        public async Task<(IReadOnlyDictionary<CourseStatus, int> CourseRunCounts, int ApprenticeshipCount, int VenueCount)> Execute(
            SqlTransaction transaction,
            GetProviderDashboardCounts query)
        {
            var sql = @"
DECLARE @ProviderUkprn INT

SELECT TOP 1 @ProviderUkprn = Ukprn FROM Pttcd.Providers WHERE ProviderId = @ProviderId

SELECT		cr.CourseRunStatus, COUNT(*) as Count
FROM		Pttcd.CourseRuns cr
INNER JOIN	Pttcd.Courses c ON c.CourseId = cr.CourseId
WHERE		c.ProviderUkprn = @ProviderUkprn
GROUP BY	cr.CourseRunStatus

SELECT COUNT(*) FROM Pttcd.Apprenticeships a
WHERE a.ProviderUkprn = @ProviderUkprn AND a.ApprenticeshipStatus & 1 <> 0

SELECT COUNT(*) FROM Pttcd.Venues v
WHERE v.ProviderUkprn = @ProviderUkprn AND v.VenueStatus = 1";

            using (var reader = await transaction.Connection.QueryMultipleAsync(sql, query, transaction))
            {
                var courseRunCounts = reader.Read().ToDictionary(r => (CourseStatus)r.CourseRunStatus, r => (int)r.Count);
                var apprenticeshipCount = reader.ReadSingle<int>();
                var venueCount = reader.ReadSingle<int>();

                return (courseRunCounts, apprenticeshipCount, venueCount);
            }
        }
    }
}
