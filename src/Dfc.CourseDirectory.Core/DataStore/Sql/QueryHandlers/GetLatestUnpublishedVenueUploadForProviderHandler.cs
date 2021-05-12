using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLatestUnpublishedVenueUploadForProviderHandler :
        ISqlQueryHandler<GetLatestUnpublishedVenueUploadForProvider, VenueUpload>
    {
        public Task<VenueUpload> Execute(SqlTransaction transaction, GetLatestUnpublishedVenueUploadForProvider query)
        {
            var sql = $@"
SELECT TOP 1 VenueUploadId, ProviderId, UploadStatus, CreatedOn, CreatedByUserId,
ProcessingStartedOn, ProcessingCompletedOn, PublishedOn, AbandonedOn, LastValidated
FROM Pttcd.VenueUploads WITH (HOLDLOCK)
WHERE ProviderId = @ProviderId
AND UploadStatus IN ({string.Join(", ", UploadStatusExtensions.UnpublishedStatuses.Cast<int>())})
ORDER BY CreatedOn DESC";

            return transaction.Connection.QuerySingleOrDefaultAsync<VenueUpload>(
                sql,
                new { query.ProviderId },
                transaction);
        }
    }
}
