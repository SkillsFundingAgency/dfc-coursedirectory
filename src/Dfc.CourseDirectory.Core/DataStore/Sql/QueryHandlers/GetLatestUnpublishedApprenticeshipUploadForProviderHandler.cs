using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLatestUnpublishedApprenticeshipUploadForProviderHandler :
        ISqlQueryHandler<GetLatestUnpublishedApprenticeshipUploadForProvider, ApprenticeshipUpload>
    {
        public Task<ApprenticeshipUpload> Execute(SqlTransaction transaction, GetLatestUnpublishedApprenticeshipUploadForProvider query)
        {
            var sql = $@"
SELECT TOP 1 ApprenticeshipUploadId, ProviderId, UploadStatus, CreatedOn, CreatedByUserId,
ProcessingStartedOn, ProcessingCompletedOn, PublishedOn, AbandonedOn
FROM Pttcd.ApprenticeshipUploads WITH (HOLDLOCK)
WHERE ProviderId = @ProviderId
AND UploadStatus IN ({string.Join(", ", UploadStatusExtensions.UnpublishedStatuses.Cast<int>())})
ORDER BY CreatedOn DESC";

            return transaction.Connection.QuerySingleOrDefaultAsync<ApprenticeshipUpload>(
                sql,
                new { query.ProviderId },
                transaction);
        }
    }
}
