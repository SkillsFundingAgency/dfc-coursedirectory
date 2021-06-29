using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;
using static Dapper.SqlMapper;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class SetCampaignProviderCoursesHandler : ISqlQueryHandler<SetCampaignProviderCourses, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, SetCampaignProviderCourses query)
        {
            var sql = @"
MERGE Pttcd.CampaignProviderCourses AS target
USING (SELECT @CampaignCode CampaignCode, ProviderUkprn, LearnAimRef FROM @Records) AS source
ON target.CampaignCode = source.CampaignCode AND target.ProviderUkprn = source.ProviderUkprn AND target.LearnAimRef = source.LearnAimRef
WHEN NOT MATCHED THEN INSERT (CampaignCode, ProviderUkprn, LearnAimRef) VALUES (source.CampaignCode, source.ProviderUkprn, source.LearnAimRef)
WHEN NOT MATCHED BY SOURCE AND target.CampaignCode = @CampaignCode THEN DELETE;";

            var paramz = new
            {
                query.CampaignCode,
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
