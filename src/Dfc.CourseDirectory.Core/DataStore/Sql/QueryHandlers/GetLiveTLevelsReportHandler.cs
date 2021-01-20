using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLiveTLevelsReportHandler : ISqlAsyncEnumerableQueryHandler<GetLiveTLevelsReport, LiveTLevelsReportItem>
    {
        public async IAsyncEnumerable<LiveTLevelsReportItem> Execute(SqlTransaction transaction, GetLiveTLevelsReport query)
        {
            var sql = @$"
SELECT      p.Ukprn,
            p.ProviderName,
            d.Name as TLevelName,
            v.VenueName,
            t.StartDate
FROM        Pttcd.TLevels t with (nolock)
INNER JOIN  Pttcd.TLevelLocations l with (nolock) ON l.TLevelId = t.TLevelId
INNER JOIN  Pttcd.TLevelDefinitions d with (nolock) ON d.TLevelDefinitionId = t.TLevelDefinitionId
INNER JOIN  Pttcd.Providers p with (nolock) ON p.ProviderId = t.ProviderId
INNER JOIN  Pttcd.Venues v with (nolock) ON v.VenueId = l.VenueId
WHERE       t.TLevelStatus = {(int)TLevelStatus.Live}
AND         l.TLevelLocationStatus = {(int)TLevelLocationStatus.Live}
ORDER BY    p.UKPRN ASC, d.Name ASC, v.VenueName ASC, t.StartDate ASC";

            using (var reader = await transaction.Connection.ExecuteReaderAsync(sql, transaction: transaction))
            {
                var parser = reader.GetRowParser<LiveTLevelsReportItem>();
                while (await reader.ReadAsync())
                {
                    yield return parser(reader);
                }
            }
        }
    }
}
