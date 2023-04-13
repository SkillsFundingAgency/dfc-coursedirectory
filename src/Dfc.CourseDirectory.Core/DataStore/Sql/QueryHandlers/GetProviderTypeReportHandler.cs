using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetProviderTypeReportHandler : ISqlAsyncEnumerableQueryHandler<GetProviderTypeReport, ProviderTypeReportItem>
    {
        public async IAsyncEnumerable<ProviderTypeReportItem> Execute(SqlTransaction transaction, GetProviderTypeReport query)
        {
            var sql = @$"
SELECT      p.Ukprn as ProviderUkprn,
            p.ProviderName,
            case 
            when p.ProviderType=3 then 1
            when p.ProviderType=6 then 4
            when p.ProviderType=7 then 5
            else p.ProviderType
            end as ProviderType,
            p.ProviderStatus,
            p.UkrlpProviderStatusDescription,
            (
                SELECT      COUNT(*)
                FROM        Pttcd.Courses c with (nolock)
                INNER JOIN  Pttcd.CourseRuns cr with (nolock) ON cr.CourseId = c.CourseId
                WHERE       c.ProviderUkprn = p.Ukprn
                AND         cr.CourseRunStatus = ${(int)CourseStatus.Live}
            ) AS LiveCourseCount,
            (
                SELECT      COUNT(*)
                FROM        Pttcd.Courses c with (nolock)
                INNER JOIN  Pttcd.CourseRuns cr with (nolock) ON cr.CourseId = c.CourseId
                WHERE       c.ProviderUkprn = p.Ukprn
                AND         cr.CourseRunStatus <> ${(int)CourseStatus.Live}
            ) AS OtherCourseCount,
            (
                SELECT      COUNT(*)
                FROM        Pttcd.TLevels t with (nolock)
                WHERE       t.ProviderId = p.ProviderId
                AND         t.TLevelStatus = ${(int)TLevelStatus.Live}
            ) AS LiveTLevelCount,
            (
                SELECT      COUNT(*)
                FROM        Pttcd.TLevels t with (nolock)
                WHERE       t.ProviderId = p.ProviderId
                AND         t.TLevelStatus <> ${(int)TLevelStatus.Live}
            ) AS OtherTLevelCount
FROM        Pttcd.Providers p with (nolock)
WHERE       p.Ukprn NOT LIKE '9%'
AND p.ProviderType <> 0  and p.ProviderType <> 2
ORDER BY    p.Ukprn ASC";

            using (var reader = await transaction.Connection.ExecuteReaderAsync(sql, transaction: transaction))
            {
                var parser = reader.GetRowParser<ProviderTypeReportItem>();
                while (await reader.ReadAsync())
                {
                    yield return parser(reader);
                }
            }
        }
    }
}
