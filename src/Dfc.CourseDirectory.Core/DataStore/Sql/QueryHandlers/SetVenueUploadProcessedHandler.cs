using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class SetVenueUploadProcessedHandler : ISqlQueryHandler<SetVenueUploadProcessed, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(SqlTransaction transaction, SetVenueUploadProcessed query)
        {
            var sql = $@"
UPDATE Pttcd.VenueUploads SET
    UploadStatus = {(int)UploadStatus.Processed},
    ProcessingCompletedOn = @ProcessingCompletedOn,
    LastValidated = @ProcessingCompletedOn,
    IsValid = @IsValid
WHERE VenueUploadId = @VenueUploadId";

            var paramz = new
            {
                query.VenueUploadId,
                query.ProcessingCompletedOn,
                query.IsValid
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
