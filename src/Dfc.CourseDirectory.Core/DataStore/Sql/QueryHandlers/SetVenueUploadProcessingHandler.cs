using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class SetVenueUploadProcessingHandler : ISqlQueryHandler<SetVenueUploadProcessing, SetVenueUploadProcessingResult>
    {
        public Task<SetVenueUploadProcessingResult> Execute(SqlTransaction transaction, SetVenueUploadProcessing query)
        {
            var sql = $@"
UPDATE Pttcd.VenueUploads
SET UploadStatus = {(int)UploadStatus.Processing}, ProcessingStartedOn = @ProcessingStartedOn
WHERE VenueUploadId = @VenueUploadId
AND UploadStatus IN ({(int)UploadStatus.Created}, {(int)UploadStatus.Processing})

IF @@ROWCOUNT = 1
    SELECT 0 AS Result
ELSE IF NOT EXISTS (SELECT 1 FROM Pttcd.VenueUploads WHERE VenueUploadId = @VenueUploadId)
    SELECT 1 AS Result
ELSE
    SELECT 2 AS Result";

            var paramz = new
            {
                query.VenueUploadId,
                query.ProcessingStartedOn
            };

            return transaction.Connection.QuerySingleAsync<SetVenueUploadProcessingResult>(sql, paramz, transaction);
        }
    }
}
