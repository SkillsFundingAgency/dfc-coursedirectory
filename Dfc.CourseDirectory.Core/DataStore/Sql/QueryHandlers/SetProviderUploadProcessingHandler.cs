using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class SetProviderUploadProcessingHandler : ISqlQueryHandler<SetProviderUploadProcessing, SetProviderUploadProcessingResult>
    {
        public Task<SetProviderUploadProcessingResult> Execute(SqlTransaction transaction, SetProviderUploadProcessing query)
        {
            var sql = $@"
UPDATE Pttcd.ProviderUploads
SET UploadStatus = {(int)UploadStatus.Processing}, ProcessingStartedOn = @ProcessingStartedOn
WHERE ProviderUploadId = @ProviderUploadId
AND UploadStatus IN ({(int)UploadStatus.Created}, {(int)UploadStatus.Processing})

IF @@ROWCOUNT = 1
    SELECT 0 AS Result
ELSE IF NOT EXISTS (SELECT 1 FROM Pttcd.ProviderUploads WHERE ProviderUploadId = @ProviderUploadId)
    SELECT 1 AS Result
ELSE
    SELECT 2 AS Result";

            var paramz = new
            {
                query.ProviderUploadId,
                query.ProcessingStartedOn
            };

            return transaction.Connection.QuerySingleAsync<SetProviderUploadProcessingResult>(sql, paramz, transaction);
        }
    }
}
