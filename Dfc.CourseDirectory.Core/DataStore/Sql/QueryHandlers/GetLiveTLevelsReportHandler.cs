using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Search.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLiveTLevelsReportHandler : ISqlAsyncEnumerableQueryHandler<GetLiveTLevelsReport, LiveTLevelsReportItem>
    {
        public async IAsyncEnumerable<LiveTLevelsReportItem> Execute(SqlTransaction transaction, GetLiveTLevelsReport query)
        {
            var sql = @$"
SELECT      c.ProviderUkprn,
            p.ProviderName,
            c.QualificationCourseTitle AS TLevelName,
            c.VenueName,
            c.StartDate
FROM        Pttcd.FindACourseIndex c with (nolock)
INNER JOIN  Pttcd.Providers p with (nolock) ON p.ProviderId = c.ProviderId
WHERE       c.OfferingType = {(int)FindACourseOfferingType.TLevel}
AND         c.Live = 1
ORDER BY    c.ProviderUkprn ASC, p.ProviderName ASC, c.QualificationCourseTitle ASC, c.VenueName ASC, c.StartDate ASC";

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
