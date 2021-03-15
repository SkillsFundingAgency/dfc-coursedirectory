using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Search.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLiveCourseProvidersReportHandler
        : ISqlAsyncEnumerableQueryHandler<GetLiveCourseProvidersReport, LiveCourseProvidersReportItem>
    {
        public async IAsyncEnumerable<LiveCourseProvidersReportItem> Execute(SqlTransaction transaction, GetLiveCourseProvidersReport query)
        {
            var sql = @$"
SELECT
            p.Ukprn,
            p.ProviderName,
            p.TradingName,
            pc.AddressSaonDescription,
            pc.AddressPaonDescription,
            pc.AddressStreetDescription,
            pc.AddressLocality,
            pc.AddressItems,
            pc.AddressPostTown,
            pc.AddressCounty,
            pc.AddressPostcode,
            pc.Telephone1,
            pc.Telephone2,
            pc.WebsiteAddress,
            pc.Email
FROM        Pttcd.Providers p with(nolock)
INNER JOIN  Pttcd.ProviderContacts pc with(nolock) ON pc.ProviderId = p.ProviderId
WHERE       p.ProviderId IN(
                SELECT      DISTINCT c.ProviderId FROM [Pttcd].[FindACourseIndex] c
                WHERE       c.OfferingType = {(int) FindACourseOfferingType.Course}
                AND         c.Live = 1
                AND         (c.FlexibleStartDate = 1 OR c.StartDate >= '{string.Format("dd-MM-yyyy", query.FromDate)}'));
ORDER BY    p.Ukprn ASC";

            using (var reader = await transaction.Connection.ExecuteReaderAsync(sql, transaction: transaction))
            {
                var parser = reader.GetRowParser<LiveCourseProvidersReportItem>();
                while (await reader.ReadAsync())
                {
                    yield return parser(reader);
                }
            }
        }
    }
}
