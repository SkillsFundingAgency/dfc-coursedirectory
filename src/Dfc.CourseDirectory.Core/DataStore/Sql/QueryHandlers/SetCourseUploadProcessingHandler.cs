using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class SetCourseUploadProcessingHandler : ISqlQueryHandler<SetCourseUploadProcessing, SetCourseUploadProcessingResult>
    {
        public Task<SetCourseUploadProcessingResult> Execute(SqlTransaction transaction, SetCourseUploadProcessing query)
        {
            var sql = $@"
UPDATE Pttcd.CourseUploads
SET UploadStatus = {(int)UploadStatus.Processing}, ProcessingStartedOn = @ProcessingStartedOn
WHERE CourseUploadId = @CourseUploadId
AND UploadStatus IN ({(int)UploadStatus.Created}, {(int)UploadStatus.Processing})

IF @@ROWCOUNT = 1
    SELECT 0 AS Result
ELSE IF NOT EXISTS (SELECT 1 FROM Pttcd.CourseUploads WHERE CourseUploadId = @CourseUploadId)
    SELECT 1 AS Result
ELSE
    SELECT 2 AS Result";

            var paramz = new
            {
                query.CourseUploadId,
                query.ProcessingStartedOn
            };

            return transaction.Connection.QuerySingleAsync<SetCourseUploadProcessingResult>(sql, paramz, transaction);
        }
    }
}
