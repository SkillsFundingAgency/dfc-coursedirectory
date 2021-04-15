using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetProviderMissingPrimaryContactReportHandler : ISqlAsyncEnumerableQueryHandler<GetProviderMissingPrimaryContactReport, ProviderMissingPrimaryContactReportItem>
    {
        public async IAsyncEnumerable<ProviderMissingPrimaryContactReportItem> Execute(SqlTransaction transaction, GetProviderMissingPrimaryContactReport query)
        {
            var sql = @$"
SELECT DISTINCT p.Ukprn as ProviderUkprn,
                p.ProviderName,
                p.ProviderType,
                p.ProviderStatus
FROM        Pttcd.Providers p
LEFT JOIN [Pttcd].[ProviderContacts] pc on p.ProviderID = pc.ProviderId
LEFT OUTER JOIN (SELECT	COUNT(1) as LiveCourseCount, 
							ProviderUkprn
                 FROM		[Pttcd].[Courses] c
			     WHERE		c.[CourseStatus] = ${(int)CourseStatus.Live}
			     GROUP BY	ProviderUkprn) cu on cu.ProviderUkprn = p.Ukprn
LEFT OUTER JOIN ( SELECT    COUNT(1) AS LiveTLevelCount, 
							[ProviderId]
                FROM        Pttcd.TLevels t
                WHERE       t.TLevelStatus = ${(int)TLevelStatus.Live}
				GROUP BY    [ProviderId]
            ) tl on tl.ProviderId = p.ProviderId
LEFT OUTER JOIN ( SELECT    COUNT(1) AS LiveApprenticeshipCount, 
							[ProviderId]
                FROM        Pttcd.Apprenticeships a
                WHERE       a.ApprenticeshipStatus = ${(int)ApprenticeshipStatus.Live}
				GROUP BY    [ProviderId]
            ) ap on ap.ProviderId = p.ProviderId
WHERE (pc.AddressPostcode IS NULL OR pc.AddressSaonDescription IS NULL)
AND   pc.ContactType = 'P'
AND	  p.ProviderStatus = ${(int)ProviderStatus.Onboarded}
AND    (
		cu.LiveCourseCount			  > 0 
        OR tl.LiveTLevelCount         > 0 
	    OR ap.LiveApprenticeshipCount > 0
	    )
ORDER BY p.Ukprn ASC
";

            using (var reader = await transaction.Connection.ExecuteReaderAsync(sql, transaction: transaction))
            {
                var parser = reader.GetRowParser<ProviderMissingPrimaryContactReportItem>();
                while (await reader.ReadAsync())
                {
                    yield return parser(reader);
                }
            }
        }
    }
}
