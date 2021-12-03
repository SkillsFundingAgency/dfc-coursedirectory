﻿using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries.OpenData;
using Dfc.CourseDirectory.Core.Search.Models;

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
            p.TradingName,
            CONCAT_WS(', ', pc.AddressSaonDescription, pc.AddressPaonDescription, pc.AddressStreetDescription) AS [ContactAddress1],
            pc.AddressLocality AS [ContactAddress2],
            ISNULL(pc.AddressPostTown, pc.AddressItems) AS [AddressPostTown],
            pc.AddressCounty,
            pc.AddressPostcode,
            ISNULL(pc.Telephone1, pc.Telephone2) AS [Telephone],
            pc.WebsiteAddress,
            pc.Email
FROM        Pttcd.Providers p with(nolock)
INNER JOIN  Pttcd.ProviderContacts pc with(nolock) ON pc.ProviderId = p.ProviderId
WHERE       p.ProviderId IN(
                SELECT      DISTINCT c.ProviderId FROM [Pttcd].[FindACourseIndex] c
                WHERE       c.Live = 1
                AND         (c.FlexibleStartDate = 1 OR c.StartDate >= '{query.FromDate:MM-dd-yyyy}')
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
