using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class SetCourseUploadProcessedHandler : ISqlQueryHandler<SetCourseUploadProcessed, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, SetCourseUploadProcessed query)
        {
            var sql = @"
UPDATE Pttcd.CourseUploads SET
    UploadStatus = @UploadStatus,
    ProcessingCompletedOn = ISNULL(ProcessingCompletedOn, @ProcessingCompletedOn),
    LastValidated = @ProcessingCompletedOn
WHERE CourseUploadId = @CourseUploadId";

            var paramz = new
            {
                query.CourseUploadId,
                query.ProcessingCompletedOn,
                UploadStatus = query.IsValid ? UploadStatus.ProcessedSuccessfully : UploadStatus.ProcessedWithErrors
            };

            var updated = await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            if (updated == 1)
            {
                return new Success();
            }
            else
            {
                return new NotFound();
            }
        }
    }
}
