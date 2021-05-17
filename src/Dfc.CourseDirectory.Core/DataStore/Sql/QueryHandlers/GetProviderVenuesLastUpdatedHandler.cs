using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetProviderVenuesLastUpdatedHandler : ISqlQueryHandler<GetProviderVenuesLastUpdated, DateTime?>
    {
        public async Task<DateTime?> Execute(SqlTransaction transaction, GetProviderVenuesLastUpdated query)
        {
            var sql = @"
DECLARE @ProviderUkprn INT
SELECT @ProviderUkprn = Ukprn FROM Pttcd.Providers WHERE ProviderId = @ProviderId

SELECT MAX(LastSyncedFromCosmos)
FROM Pttcd.Courses WITH (HOLDLOCK)
WHERE ProviderUkprn = @ProviderUkprn

SELECT MAX(LastSyncedFromCosmos)
FROM Pttcd.Apprenticeships WITH (HOLDLOCK)
WHERE ProviderUkprn = @ProviderUkprn

SELECT MAX(ISNULL(DeletedOn, UpdatedOn))
FROM Pttcd.TLevels WITH (HOLDLOCK)
WHERE ProviderId = @ProviderId

SELECT MAX(UpdatedOn)
FROM Pttcd.Venues WITH (HOLDLOCK)
WHERE ProviderUkprn = @ProviderUkprn";

            using (var reader = await transaction.Connection.QueryMultipleAsync(sql, new { query.ProviderId }, transaction))
            {
                var lastCourseUpdate = await reader.ReadSingleOrDefaultAsync<DateTime?>();
                var lastApprenticeshipUpdate = await reader.ReadSingleOrDefaultAsync<DateTime?>();
                var lastTLevelUpdate = await reader.ReadSingleOrDefaultAsync<DateTime?>();
                var lastVenueUpdate = await reader.ReadSingleOrDefaultAsync<DateTime?>();

                return new[] { lastCourseUpdate, lastApprenticeshipUpdate, lastTLevelUpdate, lastVenueUpdate }.Max();
            }
        }
    }
}
