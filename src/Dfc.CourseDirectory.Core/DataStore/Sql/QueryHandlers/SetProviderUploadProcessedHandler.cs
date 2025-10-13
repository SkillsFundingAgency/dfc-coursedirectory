using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class SetProviderUploadProcessedHandler : ISqlQueryHandler<SetProviderUploadProcessed, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, SetProviderUploadProcessed query)
        {
            var sql = @"
UPDATE Pttcd.ProviderUploads SET
    UploadStatus = @UploadStatus,
    ProcessingCompletedOn = ISNULL(ProcessingCompletedOn, @ProcessingCompletedOn)
WHERE ProviderUploadId = @ProviderUploadId";

            var paramz = new
            {
                query.ProviderUploadId,
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
