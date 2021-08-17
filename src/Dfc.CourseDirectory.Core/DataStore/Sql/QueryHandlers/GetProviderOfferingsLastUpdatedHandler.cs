using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetProviderOfferingsLastUpdatedHandler : ISqlQueryHandler<GetProviderOfferingsLastUpdated, DateTime?>
    {
        public async Task<DateTime?> Execute(SqlTransaction transaction, GetProviderOfferingsLastUpdated query)
        {
            var sql = @"
SELECT MAX(ISNULL(UpdatedOn, CreatedOn))
FROM Pttcd.Courses WITH (HOLDLOCK)
WHERE ProviderId = @ProviderId

SELECT MAX(ISNULL(UpdatedOn, CreatedOn))
FROM Pttcd.Apprenticeships WITH (HOLDLOCK)
WHERE ProviderId = @ProviderId

SELECT MAX(ISNULL(DeletedOn, UpdatedOn))
FROM Pttcd.TLevels WITH (HOLDLOCK)
WHERE ProviderId = @ProviderId

SELECT MAX(ISNULL(UpdatedOn, CreatedOn))
FROM Pttcd.Venues WITH (HOLDLOCK)
WHERE ProviderId = @ProviderId";

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
