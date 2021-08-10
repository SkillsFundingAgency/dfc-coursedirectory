using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class RemoveStaleCampaignProviderCoursesHandler : ISqlQueryHandler<RemoveStaleCampaignProviderCourses, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, RemoveStaleCampaignProviderCourses query)
        {
            var sql = @"
DECLARE @Changes TABLE (
    ProviderUkprn INT,
    LearnAimRef VARCHAR(50)
)

DELETE FROM Pttcd.CampaignProviderCourses
OUTPUT deleted.ProviderUkprn, deleted.LearnAimRef INTO @Changes
WHERE CampaignCode = @CampaignCode AND ImportJobId <> @ImportJobId
;

-- Below here should be kept in sync with UpsertCampaignProviderCoursesHandler

WITH AmendedProviderLearnAimRefs AS (
    SELECT p.ProviderId, c.LearnAimRef, c.ProviderUkprn
    FROM @Changes c
    JOIN Pttcd.Providers p ON c.ProviderUkprn = p.Ukprn
)
MERGE Pttcd.FindACourseIndexCampaignCodes AS target
USING (
    SELECT
	    x.ProviderId,
	    x.LearnAimRef,
        CONCAT('[', STRING_AGG(CASE WHEN cpc.CampaignCode IS NOT NULL THEN CONCAT('""', REPLACE(cpc.CampaignCode, '""', '\""'), '""') ELSE '' END, ','), ']') CampaignCodesJson
    FROM AmendedProviderLearnAimRefs x
    LEFT JOIN pttcd.CampaignProviderCourses cpc ON cpc.LearnAimRef = x.LearnAimRef AND cpc.ProviderUkprn = x.ProviderUkprn
    GROUP BY x.ProviderId, x.LearnAimRef
) AS source
ON target.ProviderId = source.ProviderId AND target.LearnAimRef = source.LearnAimRef
WHEN NOT MATCHED THEN INSERT (ProviderId, LearnAimRef, CampaignCodesJson) VALUES (source.ProviderId, source.LearnAimRef, source.CampaignCodesJson)
WHEN MATCHED THEN UPDATE SET CampaignCodesJson = source.CampaignCodesJson
;

MERGE Pttcd.FindACourseIndex AS target
USING (
    SELECT cc.ProviderId, cc.LearnAimRef, cc.CampaignCodesJson
    FROM @Changes c
    JOIN Pttcd.Providers p ON c.ProviderUkprn = p.Ukprn
    JOIN Pttcd.FindACourseIndexCampaignCodes cc ON p.ProviderId = cc.ProviderId AND c.LearnAimRef = cc.LearnAimRef
) AS source
ON target.ProviderId = source.ProviderId AND target.LearnAimRef = source.LearnAimRef AND target.Live = 1
WHEN MATCHED THEN UPDATE SET CampaignCodes = source.CampaignCodesJson
;";

            var paramz = new
            {
                query.CampaignCode,
                query.ImportJobId
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new Success();
        }
    }
}
