using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLatestIncompleteVenueUploadForProviderHandler :
        ISqlQueryHandler<GetLatestIncompleteVenueUploadForProvider, VenueUpload>
    {
        public Task<VenueUpload> Execute(SqlTransaction transaction, GetLatestIncompleteVenueUploadForProvider query)
        {
            var sql = $@"
SELECT TOP 1 VenueUploadId, ProviderId, UploadStatus, CreatedOn, CreatedByUserId,
ProcessingStartedOn, ProcessingCompletedOn, PublishedOn, AbandonedOn, LastValidated, IsValid
FROM Pttcd.VenueUploads
WHERE ProviderId = @ProviderId
AND UploadStatus IN ({(int)UploadStatus.Created}, {(int)UploadStatus.InProgress}, {(int)UploadStatus.Processed})
ORDER BY CreatedOn DESC";

            return transaction.Connection.QuerySingleOrDefaultAsync<VenueUpload>(sql, new { query.ProviderId }, transaction);
        }
    }
}
