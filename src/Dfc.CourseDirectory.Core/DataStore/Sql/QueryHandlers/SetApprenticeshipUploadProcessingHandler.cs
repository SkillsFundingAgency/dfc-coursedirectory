using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class SetApprenticeshipUploadProcessingHandler : ISqlQueryHandler<SetApprenticeshipUploadProcessing, SetApprenticeshipUploadProcessingResult>
    {
        public Task<SetApprenticeshipUploadProcessingResult> Execute(SqlTransaction transaction, SetApprenticeshipUploadProcessing query)
        {
            var sql = $@"
UPDATE Pttcd.ApprenticeshipUploads
SET UploadStatus = {(int)UploadStatus.Processing}, ProcessingStartedOn = @ProcessingStartedOn
WHERE ApprenticeshipUploadId = @ApprenticeshipUploadId
AND UploadStatus IN ({(int)UploadStatus.Created}, {(int)UploadStatus.Processing})

IF @@ROWCOUNT = 1
    SELECT 0 AS Result
ELSE IF NOT EXISTS (SELECT 1 FROM Pttcd.ApprenticeshipUploads WHERE ApprenticeshipUploadId = @ApprenticeshipUploadId)
    SELECT 1 AS Result
ELSE
    SELECT 2 AS Result";

            var paramz = new
            {
                query.ApprenticeshipUploadId,
                query.ProcessingStartedOn
            };

            return transaction.Connection.QuerySingleAsync<SetApprenticeshipUploadProcessingResult>(sql, paramz, transaction);
        }
    }
}
