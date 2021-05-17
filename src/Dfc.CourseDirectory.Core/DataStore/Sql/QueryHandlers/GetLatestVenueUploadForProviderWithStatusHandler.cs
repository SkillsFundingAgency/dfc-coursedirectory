using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLatestVenueUploadForProviderWithStatusHandler :
        ISqlQueryHandler<GetLatestVenueUploadForProviderWithStatus, VenueUpload>
    {
        public Task<VenueUpload> Execute(SqlTransaction transaction, GetLatestVenueUploadForProviderWithStatus query)
        {
            var sql = $@"
SELECT TOP 1 VenueUploadId, ProviderId, UploadStatus, CreatedOn, CreatedByUserId,
ProcessingStartedOn, ProcessingCompletedOn, PublishedOn, AbandonedOn, LastValidated
FROM Pttcd.VenueUploads WITH (HOLDLOCK)
WHERE ProviderId = @ProviderId
AND UploadStatus IN @Statuses
ORDER BY CreatedOn DESC";

            return transaction.Connection.QuerySingleOrDefaultAsync<VenueUpload>(
                sql,
                new { query.ProviderId, query.Statuses },
                transaction);
        }
    }
}
