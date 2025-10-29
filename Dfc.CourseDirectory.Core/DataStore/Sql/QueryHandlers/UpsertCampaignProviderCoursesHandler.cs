using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;
using static Dapper.SqlMapper;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertCampaignProviderCoursesHandler : ISqlQueryHandler<UpsertCampaignProviderCourses, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, UpsertCampaignProviderCourses query)
        {
            var sql = @"
DECLARE @Changes TABLE (
    Action NVARCHAR(10),
    ProviderUkprn INT,
    LearnAimRef VARCHAR(50)
)

MERGE Pttcd.CampaignProviderCourses AS target
USING (SELECT DISTINCT @CampaignCode CampaignCode, ProviderUkprn, LearnAimRef FROM @Records) AS source
ON target.CampaignCode = source.CampaignCode AND target.ProviderUkprn = source.ProviderUkprn AND target.LearnAimRef = source.LearnAimRef
WHEN NOT MATCHED THEN INSERT (CampaignCode, ProviderUkprn, LearnAimRef, ImportJobId) VALUES (source.CampaignCode, source.ProviderUkprn, source.LearnAimRef, @ImportJobId)
WHEN MATCHED THEN UPDATE SET ImportJobId = @ImportJobId
OUTPUT $action, inserted.ProviderUkprn, inserted.LearnAimRef INTO @Changes
;

DELETE FROM @Changes WHERE Action <> 'INSERT'
;

-- Below here should be kept in sync with RemoveStaleCampaignProviderCourses

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
                query.ImportJobId,
                Records = CreateRecordsTvp()
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new Success();

            ICustomQueryParameter CreateRecordsTvp()
            {
                var table = new DataTable();
                table.Columns.Add("ProviderUkprn", typeof(int));
                table.Columns.Add("LearnAimRef", typeof(string));

                foreach (var record in query.Records)
                {
                    table.Rows.Add(
                        record.ProviderUkprn,
                        record.LearnAimRef);
                }

                return table.AsTableValuedParameter("Pttcd.ProviderLearnAimRefTable");
            }
        }
    }
}
