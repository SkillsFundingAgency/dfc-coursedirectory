using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetApprenticeshipUploadHandler : ISqlQueryHandler<GetApprenticeshipUpload, ApprenticeshipUpload>
    {
        public Task<ApprenticeshipUpload> Execute(SqlTransaction transaction, GetApprenticeshipUpload query)
        {
            var sql = $@"
SELECT ApprenticeshipUploadId, ProviderId, UploadStatus, CreatedOn, CreatedByUserId,
ProcessingStartedOn, ProcessingCompletedOn, PublishedOn, AbandonedOn
FROM Pttcd.ApprenticeshipUploads WITH (HOLDLOCK)
WHERE ApprenticeshipUploadId = @ApprenticeshipUploadId";

            return transaction.Connection.QuerySingleOrDefaultAsync<ApprenticeshipUpload>(sql, new { query.ApprenticeshipUploadId }, transaction);
        }
    }
}
