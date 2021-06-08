using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLatestUnpublishedCourseUploadForProviderHandler :
        ISqlQueryHandler<GetLatestUnpublishedCourseUploadForProvider, CourseUpload>
    {
        public Task<CourseUpload> Execute(SqlTransaction transaction, GetLatestUnpublishedCourseUploadForProvider query)
        {
            var sql = $@"
SELECT TOP 1 CourseUploadId, ProviderId, UploadStatus, CreatedOn, CreatedByUserId,
ProcessingStartedOn, ProcessingCompletedOn, PublishedOn, AbandonedOn, LastValidated
FROM Pttcd.CourseUploads WITH (HOLDLOCK)
WHERE ProviderId = @ProviderId
AND UploadStatus IN ({string.Join(", ", UploadStatusExtensions.UnpublishedStatuses.Cast<int>())})
ORDER BY CreatedOn DESC";

            return transaction.Connection.QuerySingleOrDefaultAsync<CourseUpload>(
                sql,
                new { query.ProviderId },
                transaction);
        }
    }
}
