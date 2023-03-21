using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries.OpenData;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers.OpenData
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
            ISNULL(p.TradingName, ''),
            ISNULL(CONCAT_WS(', ', pc.AddressSaonDescription, pc.AddressPaonDescription, pc.AddressStreetDescription), '') AS[ContactAddress1],
            ISNULL(pc.AddressLocality, '') AS[ContactAddress2],
            ISNULL(pc.AddressPostTown, ISNULL(pc.AddressItems, '')) AS [AddressPostTown],
            ISNULL(pc.AddressCounty, '') AS AddressCounty,
            ISNULL(pc.AddressPostcode, '') AS AddressPostcode,
            ISNULL(pc.Telephone1, ISNULL(pc.Telephone2, '')) AS [Telephone],
            ISNULL(pc.WebsiteAddress, '') AS WebsiteAddress,
            ISNULL(pc.Email, '') AS Email
FROM        Pttcd.Providers p with(nolock)
LEFT JOIN  Pttcd.ProviderContacts pc with(nolock) ON pc.ProviderId = p.ProviderId
WHERE       p.ProviderId IN(
                SELECT      DISTINCT c.ProviderId FROM [Pttcd].[FindACourseIndex] c
                WHERE       c.Live = 1
                AND         (c.FlexibleStartDate = 1 OR c.StartDate >= '{query.FromDate:MM-dd-yyyy}')
                and [OfferingType]=1
            )
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
